using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct MakeAMessSystem : ISystem
{
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessMakingMesses
      {
         deltaTime = SystemAPI.Time.DeltaTime,
         Ecb = ecb
      }.ScheduleParallel();
   }


   public partial struct ProcessMakingMesses : IJobEntity
   {
      public float deltaTime;
      public EntityCommandBuffer.ParallelWriter Ecb;

      private void Execute([ChunkIndexInQuery] int chunkIndex, ref MakesAMessData mess, in LocalToWorld transform)
      {
         if(mess.RemainingTime > 0)
         {
            mess.RemainingTime -= deltaTime;
         }
         else
         {
            mess.RemainingTime = mess.BaseTime;
            Entity poop = Ecb.Instantiate(chunkIndex, mess.MessEntity);
            Ecb.SetComponent(chunkIndex, poop, LocalTransform.FromPositionRotationScale(transform.Position, Quaternion.identity, .1f));
         }
      }
   }
}