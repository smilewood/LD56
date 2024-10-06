using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct MovmentToDestinationSystem : ISystem
{
   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessMoveJob
      {
         deltaTime = SystemAPI.Time.DeltaTime,
         Ecb = ecb,
         foodStations = SystemAPI.GetComponentLookup<FoodStationData>(),
         drinkStations = SystemAPI.GetComponentLookup<DrinkStationData>(),
         capicity = SystemAPI.GetComponentLookup<DestinationCapicityData>(),
         workStations = SystemAPI.GetComponentLookup<WorkStationData>()
      }.ScheduleParallel();
   }

   [BurstCompile]
   public partial struct ProcessMoveJob : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter Ecb;
      public float deltaTime;
      [ReadOnly]
      public ComponentLookup<FoodStationData> foodStations;
      [ReadOnly]
      public ComponentLookup<DrinkStationData> drinkStations;
      [ReadOnly]
      public ComponentLookup<WorkStationData> workStations;
      [ReadOnly]
      public ComponentLookup<DestinationCapicityData> capicity;

      private void Execute([ChunkIndexInQuery] int chunkIndex, 
         in MoveSpeedData moveSpeed, 
         ref CurrentDestinationData destination, 
         ref LocalTransform world,
         Entity target)
      {
         DestinationCapicityData destCapicity = capicity[destination.destination];
         if (math.distance(world.Position, destination.destinationLocation) < destination.ApproachRadius)
         {
            Ecb.RemoveComponent<CurrentDestinationData>(chunkIndex, target);
            if (foodStations.TryGetComponent(destination.destination, out FoodStationData foodStation))
            {
               Ecb.AddComponent(chunkIndex, target, new ActivityData
               {
                  remainingTime = foodStation.EatingTime,
                  reservedSpot = destCapicity,
                  Activity = ActivityType.Eat
               });
            }
            else if (drinkStations.TryGetComponent(destination.destination, out DrinkStationData drinkStation))
            {
               Ecb.AddComponent(chunkIndex, target, new ActivityData
               {
                  remainingTime = drinkStation.DrinkTime,
                  reservedSpot = destCapicity,
                  Activity = ActivityType.Drink
               });
            }
            else if (workStations.TryGetComponent(destination.destination, out WorkStationData workStation))
            {
               Ecb.AddComponent(chunkIndex, target, new ActivityData
               {
                  remainingTime = workStation.DrinkTime,
                  reservedSpot = destCapicity,
                  Activity = ActivityType.Produce,
                  ActivityTarget = workStation.WorkResultPrefab
               });
            }
            else
            {
               Debug.LogError("We got somewhere without a work type");
            }

            Ecb.SetComponent(chunkIndex, destination.destination, new DestinationCapicityData
            {
               CurrentOccupancy = destCapicity.CurrentOccupancy + 1,
               MaxOccupency = destCapicity.MaxOccupency
            });
         }
         else if(destCapicity.OpenSlots <= 0)
         {
            //Someone took my spot, just give up
            Ecb.RemoveComponent<CurrentDestinationData>(chunkIndex, target);
         }
         else
         {
           // Debug.Log("Moving To Destination");
            Ecb.SetComponent(chunkIndex, target,
               LocalTransform.FromPosition(world.Position + math.normalize(destination.destinationLocation - world.Position) * moveSpeed.MoveSpeed * moveSpeed.MoveMult * deltaTime));
         }
      }
   }
}
