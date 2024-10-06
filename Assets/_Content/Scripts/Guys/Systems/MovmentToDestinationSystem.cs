using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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
         ref LocalToWorld world,
         ref PhysicsVelocity physicsVelocity,
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
                  Activity = ActivityType.Eat,
                  Location = destination.destinationLocation
               });
            }
            else if (drinkStations.TryGetComponent(destination.destination, out DrinkStationData drinkStation))
            {
               Ecb.AddComponent(chunkIndex, target, new ActivityData
               {
                  remainingTime = drinkStation.DrinkTime,
                  reservedSpot = destCapicity,
                  Activity = ActivityType.Drink,
                  Location = destination.destinationLocation
               });
            }
            else if (workStations.TryGetComponent(destination.destination, out WorkStationData workStation))
            {
               Ecb.AddComponent(chunkIndex, target, new ActivityData
               {
                  remainingTime = workStation.WorkTime,
                  reservedSpot = destCapicity,
                  Activity = ActivityType.Produce,
                  ActivityTarget = workStation.WorkResultPrefab,
                  Location = destination.destinationLocation
               });
            }
            else
            {
               Debug.LogError("We got somewhere without a work type");
            }
            //physicsVelocity.Linear = 0;
         }
         else if(destCapicity.OpenSlots <= 0)
         {
            //Someone took my spot, just give up
            Ecb.RemoveComponent<CurrentDestinationData>(chunkIndex, target);
         }
         else
         {
            float3 targetvelocity = math.normalizesafe(destination.destinationLocation - world.Position);
            targetvelocity.y = .1f;
            physicsVelocity.Linear += deltaTime * moveSpeed.MoveSpeed * targetvelocity;
            
         }
      }
   }
}
