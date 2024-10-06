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


   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
      EntityQuery needDestQuery = SystemAPI.QueryBuilder()
         .WithAll<DestinationDesireData>()
         .WithAbsent<ActivityData, CurrentDestinationData>().Build();
      new DestinationChoiceJob
      {
         Ecb = ecb,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>()
      }.ScheduleParallel(needDestQuery);

   }

   [BurstCompile]
   public partial struct DestinationChoiceJob : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;


      public void Execute([ChunkIndexInQuery] int chunkIndex, in DestinationDesireData desire, Entity target)
      {
         float maxWeight = -math.INFINITY;
         Entity maxTarget = default;


         if (desire.foodWeight > maxWeight)
         {
            maxTarget = desire.foodTarget;
            maxWeight = desire.foodWeight;
         }
         if (desire.waterWeight > maxWeight)
         {
            maxTarget = desire.waterTarget;
            maxWeight = desire.waterWeight;
         }
         if (desire.workWeight > maxWeight)
         {
            maxTarget = desire.workTarget;
            maxWeight = desire.workWeight;
         }



         //TODO: I think there is data somewhere to adjust the approach distance
         float approach = 1;


         if (maxWeight != -math.INFINITY)
         {
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
