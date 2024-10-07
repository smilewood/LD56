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
         workStations = SystemAPI.GetComponentLookup<WorkStationData>(),
         haulableStuff = SystemAPI.GetComponentLookup<HaulableData>(),
         cleanupSpots = SystemAPI.GetComponentLookup<CleaningSpotData>(),
         dropStations = SystemAPI.GetComponentLookup<DropPointData>()
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
      public ComponentLookup<HaulableData> haulableStuff;
      [ReadOnly]
      public ComponentLookup<DropPointData> dropStations;
      [ReadOnly]
      public ComponentLookup<CleaningSpotData> cleanupSpots;
      [ReadOnly]
      public ComponentLookup<DestinationCapicityData> capicity;

      private void Execute([ChunkIndexInQuery] int chunkIndex, 
         in MoveSpeedData moveSpeed, 
         ref CurrentDestinationData destination, 
         ref LocalToWorld world,
         ref PhysicsVelocity physicsVelocity,
         ref SpriteSheetAnimation animator,
         ref BigBrainData brain,
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
               animator.animationIndex = 5;
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
               animator.animationIndex = 5;
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
               animator.animationIndex = 3;
            }
            else if (haulableStuff.TryGetComponent(destination.destination, out HaulableData data))
            {
               Ecb.AddComponent(chunkIndex, target, new ActivityData
               {
                  remainingTime = 0.1f,
                  Activity = ActivityType.PickUp,
                  ActivityTarget = destination.destination,
                  Location = destination.destinationLocation,
                  reservedSpot = destCapicity
               });
               animator.animationIndex = 4;
            }
            else if(dropStations.TryGetComponent(destination.destination, out DropPointData dropPoint))
            {
               Ecb.AddComponent(chunkIndex, target, new ActivityData
               {
                  remainingTime = 0.1f,
                  Activity = ActivityType.DropOff,
                  ActivityTarget = destination.destination,
                  Location = destination.destinationLocation
               });
            }
            else if (cleanupSpots.TryGetComponent(destination.destination, out CleaningSpotData cleanSpot))
            {
               Ecb.AddComponent(chunkIndex, target, new ActivityData
               {
                  remainingTime = cleanSpot.CleaningTime,
                  Activity = ActivityType.Clean,
                  ActivityTarget = destination.destination,
                  Location = destination.destinationLocation,
                  reservedSpot = destCapicity
               });
            }
            else
            {
               Debug.LogError("We got somewhere without a work type");
            }
            //physicsVelocity.Linear = 0;
         }
         else if(destCapicity.OpenSlots <= 0 || brain.ReconsiderTimer <= 0)
         {
            Ecb.RemoveComponent<CurrentDestinationData>(chunkIndex, target);
            animator.animationIndex = 6;
         }
         else
         {
            float3 targetvelocity = math.normalizesafe(destination.destinationLocation - world.Position);
            targetvelocity.y = .3f;
            physicsVelocity.Linear += deltaTime * moveSpeed.MoveSpeed * targetvelocity;
         }
      }
   }
}
