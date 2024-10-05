using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct TempWanderSystem : ISystem
{
   Unity.Mathematics.Random testRandom;

   private void OnUpdate(ref SystemState state)
   {
      
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessWanderJob
      {
         deltaTime = SystemAPI.Time.DeltaTime,
         Ecb = ecb
      }.ScheduleParallel();
   }

   public partial struct ProcessWanderJob : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;
      public float deltaTime;

      private void Execute([ChunkIndexInQuery] int chunkIndex, ref MoveSpeedData moveSpeed, ref LocalTransform transform, Entity target)
      {
         //if(guy.Destination == null)
         //{
         //   //Pick a location

         //   Ecb.SetComponent(chunkIndex, target, new MovableData
         //   { 
         //      Destination = new float3(7, 0f, 7), 
         //      speed = guy.speed
         //   });
         //   return;
         //}
           
         //if(math.distance(guy.Destination.Value, transform.Position) < .1f)
         //{
         //   //Destination reached!
         //   Ecb.SetComponent(chunkIndex, target, new MovableData
         //   {
         //      Destination = guy.Destination * -1,
         //      speed = guy.speed
         //   });
         //   return;
         //}

         float3 newPos = math.lerp(transform.Position, guy.Destination.Value, guy.speed * deltaTime);
         Ecb.SetComponent(chunkIndex, target, LocalTransform.FromPosition(newPos));
      }
   }
}