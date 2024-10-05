using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public partial struct ActivitySystem : ISystem
{
   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessActivityTimer { deltaTime = SystemAPI.Time.DeltaTime, Ecb = ecb }.ScheduleParallel();
   }

   [BurstCompile]
   public partial struct ProcessActivityTimer : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;
      public float deltaTime;
      private void Execute([ChunkIndexInQuery] int chunkIndex, ref ActivityData activity, Entity target)
      {
         if(activity.remainingTime < 0)
         {
            Ecb.RemoveComponent<ActivityData>(chunkIndex, target);
            Ecb.SetComponent(chunkIndex, activity.reservedSpotEntity, new DestinationCapicityData
            {
               CurrentOccupancy = activity.reservedSpot.CurrentOccupancy - 1,
               MaxOccupency = activity.reservedSpot.MaxOccupency
            });
         }
         else
         {
            Ecb.SetComponent(chunkIndex, target, new ActivityData 
            {
               remainingTime = activity.remainingTime - deltaTime,
               reservedSpot = activity.reservedSpot,
               reservedSpotEntity = activity.reservedSpotEntity
            });
         }
      }
   }
}