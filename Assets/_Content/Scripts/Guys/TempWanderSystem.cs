//using System;
//using NUnit;
//using Unity.Collections;
//using Unity.Collections.LowLevel.Unsafe;
//using Unity.Entities;
//using Unity.Jobs.LowLevel.Unsafe;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;

//public partial struct TempWanderSystem : ISystem
//{
//   Unity.Mathematics.Random testRandom;
//   [NativeDisableContainerSafetyRestriction]
//   public NativeArray<Unity.Mathematics.Random> Randoms;

//   private void OnCreate(ref SystemState state)
//   {
//      Randoms = new NativeArray<Unity.Mathematics.Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
//      uint r = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
//      for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
//      {
//         Randoms[i] = new Unity.Mathematics.Random(r == 0 ? r + 1 : r);
//      }
//   }

//   private void OnUpdate(ref SystemState state)
//   {

//      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

//      new ProcessWanderJob
//      {
//         Ecb = ecb
//      }.ScheduleParallel();
//   }

//   public partial struct ProcessWanderJob : IJobEntity
//   {
//      [NativeDisableContainerSafetyRestriction]
//      public NativeArray<Unity.Mathematics.Random> Randoms;
//      [NativeSetThreadIndex] private int _threadId;

//      public EntityCommandBuffer.ParallelWriter ecb;

//      private void Execute([ChunkIndexInQuery] int chunkIndex, in MovableData _, Entity target)
//      {
//         Unity.Mathematics.Random random = Randoms[_threadId];
//         float3 location = new float3((random.NextFloat() * 8) - 4, 0f, (random.NextFloat() * 8) - 4);

//         ecb.AppendToBuffer(chunkIndex, target, new DestinationDesireData { target = location, weight = .5f });
//         Randoms[_threadId] = random;
//      }
//   }
//}