using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct ModifierDrainSystem : ISystem
{
   private BufferLookup<ModifierData> modifierBuffer;

   public void OnCreate(ref SystemState state)
   {
      modifierBuffer = state.GetBufferLookup<ModifierData>(false);
   }

   [BurstCompile]
   public void Update(ref SystemState state)
   {
      Debug.Log("Here");
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
      modifierBuffer.Update(ref state);
      new DrainModifierJob
      {
         DeltaTime = SystemAPI.Time.DeltaTime,
         ecb = ecb,
         modifierBuffer = modifierBuffer
      }.ScheduleParallel();
   }

   [BurstCompile]
   public partial struct DrainModifierJob : IJobEntity
   {
      public float DeltaTime;
      public EntityCommandBuffer.ParallelWriter ecb;
      [ReadOnly]
      public BufferLookup<ModifierData> modifierBuffer;

      public void Execute([ChunkIndexInQuery] int chunkIndex, in CanHaveModifiers _, Entity target)
      {
         DynamicBuffer<ModifierData> modBuffer = modifierBuffer[target];
         Debug.Log($"there are {modBuffer.Length} things in the buffer");
         for (int i = 0; i < modBuffer.Length; ++i)
         {
            var modifier = modBuffer[i];
            if(modifier.CurrentValue <= 0)
            {
               HandleModifierDrained(chunkIndex, ref modifier, ref modBuffer, target);
            }
            else
            {
               modifier.CurrentValue = math.min(modifier.CurrentValue + (modifier.naturalDecayRate * DeltaTime), modifier.MaxValue);
               modBuffer[i] = modifier;
            }
            Debug.Log($"{modBuffer[i].ModType} changed to {modBuffer[i].CurrentValue}");
         }
      }

      const float DrainRate = .1f;
      
      private void HandleModifierDrained(int chunkIndex, ref ModifierData modifier, ref DynamicBuffer<ModifierData> modBuffer, Entity target)
      {
         switch (modifier.ModType)
         {
            case ModifierType.Hunger:
            {
               for (int i = 0; i < modBuffer.Length; ++i)
               {
                  if (modBuffer[i].ModType == ModifierType.Health)
                  {
                     var healthMod = modBuffer[i];
                     healthMod.CurrentValue -= DrainRate * DeltaTime;
                     modBuffer[i] = healthMod;
                     break;
                  }
               }
               //TODO this is likely where we can set the animation?
               break;
            }
            case ModifierType.Thirst:
            {
               for (int i = 0; i < modBuffer.Length; ++i)
               {
                  if (modBuffer[i].ModType == ModifierType.Health)
                  {
                     var healthMod = modBuffer[i];
                     healthMod.CurrentValue -= DrainRate * DeltaTime;
                     modBuffer[i] = healthMod;
                     break;
                  }
               }
               //TODO this is likely where we can set the animation?
               break;
            }
            case ModifierType.Health:
            {
               ecb.DestroyEntity(chunkIndex, target);
               //TODO this is likely where we can set the animation?
               break;
            }
         }
      }
   }
}