using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct HungerDrainSystem : ISystem
{
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessHungerDrainJob
      {
         deltaTime = SystemAPI.Time.DeltaTime,
         ecb = ecb
      }.ScheduleParallel();
   }

   public partial struct ProcessHungerDrainJob : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter ecb;
      public float deltaTime;
      private void Execute([ChunkIndexInQuery] int chunkIndex, ref HaveHungerData hunger, Entity target)
      {
         ecb.SetComponent(chunkIndex, target, new HaveHungerData
         {
            CurrentHunger = hunger.CurrentHunger - (hunger.DrainPerSec * hunger.HungerDrainMult * deltaTime),
            HungerDrainMult = hunger.HungerDrainMult,
            MaxHunger = hunger.MaxHunger,
            DrainPerSec = hunger.DrainPerSec
         });
      }
   }
}

public partial struct FoodFinderSystem : ISystem
{
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      ConcurrentBag<float3> foodStationLocations = new ConcurrentBag<float3>();
      JobHandle findSourceJob = new FindFoodSourcesJob { sources = foodStationLocations }.ScheduleParallel(state.Dependency);

      new ProcessFoodFinder { ecb = ecb, sources = foodStationLocations }.ScheduleParallel(findSourceJob);

   }

   public partial struct FindFoodSourcesJob : IJobEntity
   {
      public ConcurrentBag<float3> sources;

      public void Execute(FoodStationData station, in LocalToWorld localToWorld)
      {
         if(station.CurrentOccupancy < station.MaxOccupency)
         {
           sources.Add(localToWorld.Position);
         }
      }
   }


   public partial struct ProcessFoodFinder: IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter ecb;
      public ConcurrentBag<float3> sources;


      private void Execute([ChunkIndexInQuery] int chunkIndex, in HaveHungerData hunger, ref DestinationDesireData dests,LocalToWorld transform, Entity ent)
      {
         float weight = -Mathf.Log(hunger.CurrentHunger / hunger.MaxHunger);

         (float, float3) bestTarget = (math.INFINITY, float3.zero);

         foreach(float3 target in sources)
         {
            float dist = math.abs(math.distance(transform.Position, target));
            if(dist < bestTarget.Item1)
            {
               bestTarget = (dist, target);
            }
         }

         if(weight > dests.weight)
         {
            ecb.SetBuffer(chunkIndex, ent, new DestinationDesireData { weight = weight, target = bestTarget.Item2 });
         }
      }
   }

}