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
         .WithAll<DestinationDesireData, SpriteSheetAnimation, BigBrainData>()
         .WithAbsent<ActivityData, CurrentDestinationData>().Build();
      new DestinationChoiceJob
      {
         Ecb = ecb,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>(),
         haulers = SystemAPI.GetComponentLookup<HaulerStateData>()
      }.ScheduleParallel(needDestQuery);
      new BrainTimerJob { deltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
   }

   [BurstCompile]

   public partial struct BrainTimerJob : IJobEntity
   {
      public float deltaTime;
      private void Execute(ref BigBrainData brain)
      {
         if(brain.ReconsiderTimer > 0)
         {
            brain.ReconsiderTimer -= deltaTime;
         }
      }
   }

   [BurstCompile]
   public partial struct DestinationChoiceJob : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;
      [ReadOnly]
      public ComponentLookup<HaulerStateData> haulers;


      public void Execute([ChunkIndexInQuery] int chunkIndex, in DestinationDesireData desire, ref SpriteSheetAnimation animator, ref BigBrainData brain, Entity target)
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
         float approach = 2;


         if (maxWeight != -math.INFINITY)
         {
            Ecb.AddComponent(chunkIndex, target, new CurrentDestinationData
            {
               destination = maxTarget,
               destinationLocation = locations[maxTarget].Position,
               ApproachRadius = approach
            });

            if(haulers.TryGetComponent(target, out HaulerStateData data) && data.Hauling)
            {
               animator.animationIndex = 3;
            }
            else
            {
               animator.animationIndex = 4;
            }
            brain.ReconsiderTimer = 15;
         }
      }


   }
}
