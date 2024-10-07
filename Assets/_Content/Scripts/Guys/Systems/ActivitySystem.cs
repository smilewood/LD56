using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public partial struct ActivitySystem : ISystem
{
   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessActivityTimer { deltaTime = SystemAPI.Time.DeltaTime, Ecb = ecb }.ScheduleParallel();
      new ProcessActivityChanges { deltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();

      new ProcessProducerActivity { Ecb = ecb }.ScheduleParallel();
      new ProcessHaulerActivity { Ecb = ecb, haulableStuff = SystemAPI.GetComponentLookup<HaulableData>() }.ScheduleParallel();
      new ProcessCleanerActivity { Ecb = ecb }.ScheduleParallel();

      new MoveToActivityJob { deltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();

   }

   [BurstCompile]
   public partial struct MoveToActivityJob : IJobEntity
   {
      public float deltaTime;

      private void Execute(
         in ActivityData activity, 
         in MoveSpeedData moveSpeed,
         ref LocalToWorld world,
         ref PhysicsVelocity physicsVelocity
         )
      {
         float3 targetvelocity = math.normalizesafe(activity.Location - world.Position);
         targetvelocity.y = 0;
         physicsVelocity.Linear += 0.25f * deltaTime * moveSpeed.MoveSpeed * targetvelocity;
      }

   }

   [BurstCompile]
   public partial struct ProcessProducerActivity : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;

      private void Execute([ChunkIndexInQuery] int chunkIndex, in ProducerStateData _, in ActivityData activity, ref SpriteSheetAnimation animator, in LocalToWorld transform)
      {
         if(activity.remainingTime <= 0 && activity.Activity == ActivityType.Produce)
         {
            animator.animationIndex = 5;
            Entity result = Ecb.Instantiate(chunkIndex, activity.ActivityTarget);
            Ecb.SetComponent(chunkIndex, result, LocalTransform.FromPositionRotationScale(transform.Position, Quaternion.identity, .1f));
         }
      }
   }

   [BurstCompile]
   public partial struct ProcessHaulerActivity : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;
      [ReadOnly]
      public ComponentLookup<HaulableData> haulableStuff;

      private void Execute([ChunkIndexInQuery] int chunkIndex, ref HaulerStateData hauler, ref ActivityData activity, ref SpriteSheetAnimation animator, Entity target)
      {
         if (activity.remainingTime <= 0)
         {
            if (activity.Activity == ActivityType.PickUp)
            {
               hauler.Hauling = true;
               hauler.TypeBeingHauled = haulableStuff[activity.ActivityTarget].Type;
               if (haulableStuff.EntityExists(activity.ActivityTarget))
               {
                  Ecb.DestroyEntity(chunkIndex, activity.ActivityTarget);
               }
               
               animator.animationIndex = 3;
            }
            else if(activity.Activity == ActivityType.DropOff)
            {
               hauler.Hauling = false;
               animator.animationIndex = 5;
               //This is where we need to notify the game that the hauling is complete
            }
         }
      }
   }

   [BurstCompile]
   public partial struct ProcessCleanerActivity : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;

      private void Execute([ChunkIndexInQuery] int chunkIndex, in CleanerStateData _, in ActivityData activity, ref SpriteSheetAnimation animator)
      {
         if (activity.Activity == ActivityType.Clean)
         {
            animator.animationIndex = 3;
            if (activity.remainingTime <= 0)
            {
               animator.animationIndex = 5;
               Ecb.DestroyEntity(chunkIndex, activity.ActivityTarget);
            }
         }
      }
   }

   [BurstCompile]
   public partial struct ProcessActivityTimer : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;
      public float deltaTime;
      private void Execute([ChunkIndexInQuery] int chunkIndex, ref ActivityData activity, ref SpriteSheetAnimation animator, Entity target)
      {
         if(activity.remainingTime < 0)
         {
            Ecb.RemoveComponent<ActivityData>(chunkIndex, target);
         }
         else
         {
            activity.remainingTime -= deltaTime;
         }
      }
   }

   const float EatingRate = 1f;
   const float DrinkingRate = 2f;

   [BurstCompile]
   public partial struct ProcessActivityChanges : IJobEntity
   {
      public float deltaTime;
      private void Execute(DynamicBuffer<ModifierData> modBuffer, ref ActivityData activity)
      {
         switch (activity.Activity)
         {
            case ActivityType.Eat:
            {
               for (int i = 0; i < modBuffer.Length; ++i)
               {
                  if (modBuffer[i].ModType == ModifierType.Hunger)
                  {
                     var hungerMod = modBuffer[i];
                     hungerMod.CurrentValue = math.min(hungerMod.CurrentValue + (EatingRate * deltaTime), hungerMod.MaxValue);
                     modBuffer[i] = hungerMod;
                     break;
                  }
               }
               break;
            }
            case ActivityType.Drink:
            {
               for (int i = 0; i < modBuffer.Length; ++i)
               {
                  if (modBuffer[i].ModType == ModifierType.Thirst)
                  {
                     var thirstMod = modBuffer[i];
                     thirstMod.CurrentValue = math.min(thirstMod.CurrentValue + (DrinkingRate * deltaTime), thirstMod.MaxValue);
                     modBuffer[i] = thirstMod;
                     break;
                  }
               }
               break;
            }
         }
      } 
   }
}