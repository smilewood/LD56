using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;


//public partial struct HungerDrainSystem : ISystem
//{
//   public void OnUpdate(ref SystemState state)
//   {
//      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

//      new ProcessHungerDrainJob
//      {
//         deltaTime = SystemAPI.Time.DeltaTime,
//         ecb = ecb
//      }.ScheduleParallel();
//   }

//   [BurstCompile]
//   public partial struct ProcessHungerDrainJob : IJobEntity
//   {
//      public EntityCommandBuffer.ParallelWriter ecb;
//      public float deltaTime;
//      private void Execute([ChunkIndexInQuery] int chunkIndex, ref HaveHungerData hunger, Entity target)
//      {
//         ecb.SetComponent(chunkIndex, target, new HaveHungerData
//         {
//            CurrentHunger = hunger.CurrentHunger - (hunger.DrainPerSec * hunger.HungerDrainMult * deltaTime),
//            HungerDrainMult = hunger.HungerDrainMult,
//            MaxHunger = hunger.MaxHunger,
//            DrainPerSec = hunger.DrainPerSec
//         });
//      }
//   }
//}

public partial struct FoodFinderSystem : ISystem
{
   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityQuery FoodSourceQuery = SystemAPI.QueryBuilder().WithAll<FoodStationData, DestinationCapicityData>().Build();
      NativeArray<Entity> foodStationLocations = FoodSourceQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.Handle);

      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessFoodFinder {
         ecb = ecb,
         sources = foodStationLocations,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>(),
         capicatity = SystemAPI.GetComponentLookup<DestinationCapicityData>()
      }.ScheduleParallel();

   }

   [BurstCompile]
   public partial struct ProcessFoodFinder: IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter ecb;
      public NativeArray<Entity> sources;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;
      [ReadOnly]
      public ComponentLookup<DestinationCapicityData> capicatity;

      private void Execute(ref DestinationDesireData desires, ref DynamicBuffer<ModifierData> modifiers, in LocalToWorld transform)
      {
         ModifierData hunger = default;
         for (int i = 0; i < modifiers.Length; ++i)
         {
            if (modifiers[i].ModType == ModifierType.Hunger)
            {
               hunger = modifiers[i];
               break;
            }
         }

         float weight = -(0.75f * (hunger.CurrentValue / hunger.MaxValue)) + 1;

         float bestTargetDist = math.INFINITY;
         Entity bestTarget = default;


         foreach(Entity target in sources)
         {
            if (capicatity.TryGetComponent(target, out DestinationCapicityData seats) && seats.OpenSlots > 0)
            {
               if (locations.HasComponent(target))
               {
                  LocalToWorld targetPos = locations[target];

                  float dist = math.abs(math.distance(transform.Position, targetPos.Position));
                  if (dist < bestTargetDist)
                  {
                     bestTargetDist = dist;
                     bestTarget = target;
                  }
               }
            }
         }
         if(bestTargetDist != math.INFINITY)
         {
            desires.foodWeight = weight;
            desires.foodTarget = bestTarget;
         }
         else
         {
            desires.foodWeight = -1;
         }
      }
   }

}