using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct BigBrainSystem : ISystem
{
   private BufferLookup<DestinationDesireData> desireBuffer;

   public void OnCreate(ref SystemState state)
   {
      desireBuffer = state.GetBufferLookup<DestinationDesireData>(false);
   }

   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
      EntityQuery needDestQuery = SystemAPI.QueryBuilder()
         .WithAbsent<ActivityData, CurrentDestinationData>().Build();
      desireBuffer.Update(ref state);
      new DestinationChoiceJob
      {
         desireBuffer = desireBuffer,
         Ecb = ecb,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>()
      }.ScheduleParallel(needDestQuery);
   }

   [BurstCompile]
   public partial struct DestinationChoiceJob : IJobEntity
   {
      [ReadOnly]
      public BufferLookup<DestinationDesireData> desireBuffer;

      public EntityCommandBuffer.ParallelWriter Ecb;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;


      public void Execute([ChunkIndexInQuery] int chunkIndex, Entity target)
      {
         float maxWeight = -math.INFINITY;
         Entity maxTarget = default;

         if (desireBuffer.HasBuffer(target))
         {
            foreach (DestinationDesireData d in desireBuffer[target])
            {
               if (d.weight > maxWeight)
               {
                  maxTarget = d.target;
                  maxWeight = d.weight;
               }
            }
            float approach = 1;


            if (maxWeight != -math.INFINITY)
            {
               Debug.Log($"Picking new destination {locations[maxTarget].Position}");
               Ecb.AddComponent(chunkIndex, target, new CurrentDestinationData
               {
                  destination = maxTarget,
                  destinationLocation = locations[maxTarget].Position,
                  ApproachRadius = approach
               });
            }
         }
      }


   }
}
