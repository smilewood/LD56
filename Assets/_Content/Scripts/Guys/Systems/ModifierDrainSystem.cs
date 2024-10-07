using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct ModifierDrainSystem : ISystem
{

   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
      new DrainModifierJob
      {
         DeltaTime = SystemAPI.Time.DeltaTime,
         ecb = ecb,
      }.ScheduleParallel();
      new SpawnNewGuysJob
      {
         deltaTime = SystemAPI.Time.DeltaTime,
         ecb = ecb
      }.ScheduleParallel();

   }
   private const float ReproduceThreshold = .75f;
   public partial struct SpawnNewGuysJob : IJobEntity
   {
      public float deltaTime;
      public EntityCommandBuffer.ParallelWriter ecb;

      private void Execute([ChunkIndexInQuery] int chunkIndex, in DynamicBuffer<ModifierData> initialBuffer, ref BigBrainData brain, Entity target)
      {
         for (int i = 0; i < initialBuffer.Length; ++i)
         {
            var modifier = initialBuffer[i];
            if ((modifier.CurrentValue / modifier.MaxValue) < ReproduceThreshold)
            {
               Debug.Log($"{modifier.ModType.ToString()} is under the reproduction threshold");
               brain.ReproduceTimer = brain.ReproduceCooldown;
               return;
            }
         }

         if(brain.ReproduceTimer <= 0)
         {
            brain.ReproduceTimer = brain.ReproduceCooldown;
            ecb.Instantiate(chunkIndex, target);
         }
         else
         {
            brain.ReproduceTimer -= deltaTime;
         }
      }
   }

   [BurstCompile]
   public partial struct DrainModifierJob : IJobEntity
   {
      public float DeltaTime;
      public EntityCommandBuffer.ParallelWriter ecb;

      public void Execute([ChunkIndexInQuery] int chunkIndex, ref DynamicBuffer<ModifierData> initialBuffer, Entity target)
      {
         DynamicBuffer<ModifierData> modBuffer = ecb.SetBuffer<ModifierData>(chunkIndex, target);

         for (int i = 0; i < initialBuffer.Length; ++i)
         {
            var modifier = initialBuffer[i];
            if(modifier.CurrentValue <= 0)
            {
               HandleModifierDrained(chunkIndex, ref modifier, ref initialBuffer, target);
            }
            else
            {
               modifier.CurrentValue = math.max(0, math.min(modifier.CurrentValue - (modifier.naturalDecayRate * DeltaTime), modifier.MaxValue));
               initialBuffer[i] = modifier;
            }
         }

         for (int i = 0; i < initialBuffer.Length; ++i)
         {
            modBuffer.Add(initialBuffer[i]);
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