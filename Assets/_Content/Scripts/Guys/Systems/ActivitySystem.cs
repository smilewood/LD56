using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            HandleActivityEnd(chunkIndex, ref activity);
            Ecb.RemoveComponent<ActivityData>(chunkIndex, target);
         }
         else
         {
            activity.remainingTime -= deltaTime;
         }
      }

      private void HandleActivityEnd(int chunkIndex, ref ActivityData activity)
      {
         switch (activity.Activity)
         {
            case ActivityType.Produce:
            {
               Ecb.Instantiate(chunkIndex, activity.ActivityTarget);
               break;
            }
            case ActivityType.Clean:
            {
               Ecb.DestroyEntity(chunkIndex, activity.ActivityTarget);
               break;
            }
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