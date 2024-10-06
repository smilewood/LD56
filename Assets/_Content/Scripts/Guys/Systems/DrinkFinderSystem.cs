using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct DrinkFinderSystem : ISystem
{
   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityQuery DrinkSourceQuery = SystemAPI.QueryBuilder().WithAll<DrinkStationData, DestinationCapicityData>().Build();
      NativeArray<Entity> drinkStationLocations = DrinkSourceQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.Handle);

      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessDrinkFinder
      {
         ecb = ecb,
         sources = drinkStationLocations,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>(),
         capicatity = SystemAPI.GetComponentLookup<DestinationCapicityData>()
      }.ScheduleParallel();

   }

   [BurstCompile]
   public partial struct ProcessDrinkFinder : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter ecb;
      public NativeArray<Entity> sources;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;
      [ReadOnly]
      public ComponentLookup<DestinationCapicityData> capicatity;

      private void Execute(ref DestinationDesireData desires, in DynamicBuffer<ModifierData> modifiers, in LocalToWorld transform)
      {
         ModifierData thirst = default;
         for (int i = 0; i < modifiers.Length; ++i)
         {
            if (modifiers[i].ModType == ModifierType.Thirst)
            {
               thirst = modifiers[i];
               break;
            }
         }

         float weight = -Mathf.Log(thirst.CurrentValue / thirst.MaxValue);

         float bestTargetDist = math.INFINITY;
         Entity bestTarget = default;

         foreach (Entity target in sources)
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
         if (bestTargetDist != math.INFINITY)
         {
            desires.waterWeight = weight;
            desires.waterTarget = bestTarget;
         }
         else
         {
            desires.waterWeight = -1;
         }
      }
   }

}