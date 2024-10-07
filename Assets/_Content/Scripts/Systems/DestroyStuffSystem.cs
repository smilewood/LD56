using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(BigBrainSystem))]
public partial struct DestroyStuffSystem : ISystem
{
   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessDestroyuStuff
      {
         Ecb = ecb
      }.ScheduleParallel();
   }

   [BurstCompile]
   public partial struct ProcessDestroyuStuff : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;

      private void Execute([ChunkIndexInQuery] int chunkIndex, in DestroyMe _, Entity target)
      {
         Ecb.DestroyEntity(chunkIndex, target);
      }
   }
}
