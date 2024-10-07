using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct WorkFinderSystem : ISystem
{
   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityQuery WorkSourceQuery = SystemAPI.QueryBuilder().WithAll<WorkStationData, DestinationCapicityData>().Build();
      NativeArray<Entity> workStationLocations = WorkSourceQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.Handle);

      EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

      new ProcessProducerWorkSearch
      {
         workstations = workStationLocations,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>(),
         capicatity = SystemAPI.GetComponentLookup<DestinationCapicityData>()
      }.ScheduleParallel();

      EntityQuery haulableStuffQuery = SystemAPI.QueryBuilder().WithAll<HaulableData>().Build();
      NativeArray<Entity> haulableLocations = haulableStuffQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.Handle);

      EntityQuery dropPointsQuery = SystemAPI.QueryBuilder().WithAll<DropPointData>().Build();
      NativeArray<Entity> dropPoints = dropPointsQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.Handle);

      new ProcessHaulerWorkSearch
      {
         haulableStuff = haulableLocations,
         dropPoints = dropPoints,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>()

      }.ScheduleParallel();

      EntityQuery cleanPointsQuery = SystemAPI.QueryBuilder().WithAll<CleaningSpotData>().Build();
      NativeArray<Entity> cleanPoints = cleanPointsQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.Handle);

      new ProcessCleanerWorkSearch
      {
         messes = cleanPoints,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>(),
         capicatity = SystemAPI.GetComponentLookup<DestinationCapicityData>()
      }.ScheduleParallel();

   }

   [BurstCompile]
   public partial struct ProcessHaulerWorkSearch : IJobEntity
   {
      public NativeArray<Entity> haulableStuff;
      public NativeArray<Entity> dropPoints;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;

      private void Execute(ref DestinationDesireData desires, in HaulerStateData hauler, in LocalToWorld transform)
      {
         float bestTargetDist = math.INFINITY;
         Entity bestTarget = default;

         if (!hauler.Hauling)
         {
            foreach (Entity target in haulableStuff)
            {
               if (locations.HasComponent(target))
               {
                  LocalToWorld targetPos = locations[target];

                  float dist = math.abs(math.distance(transform.Position, targetPos.Position));
                  if (dist < bestTargetDist)
                  {
                     bestTargetDist = dist;
                     bestTarget = target;
                  }
               }
            }
            if (bestTargetDist != math.INFINITY)
            {
               desires.workWeight = .5f;
               desires.workTarget = bestTarget;
            }
            else
            {
               desires.workWeight = -1;
            }
         }
         else
         {
            foreach (Entity target in dropPoints)
            {
               if (locations.HasComponent(target))
               {
                  LocalToWorld targetPos = locations[target];

                  float dist = math.abs(math.distance(transform.Position, targetPos.Position));
                  if (dist < bestTargetDist)
                  {
                     bestTargetDist = dist;
                     bestTarget = target;
                  }
               }
            }
            if (bestTargetDist != math.INFINITY)
            {
               desires.workWeight = .8f;
               desires.workTarget = bestTarget;
            }
            else
            {
               desires.workWeight = -1;
            }
         }
      }
   }


   [BurstCompile]
   public partial struct ProcessProducerWorkSearch : IJobEntity
   {
      public NativeArray<Entity> workstations;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;
      [ReadOnly]
      public ComponentLookup<DestinationCapicityData> capicatity;

      private void Execute(ref DestinationDesireData desires, in ProducerStateData _, in LocalToWorld transform)
      {
         float bestTargetDist = math.INFINITY;
         Entity bestTarget = default;

         foreach (Entity target in workstations)
         {

            if (capicatity.TryGetComponent(target, out DestinationCapicityData seats) && seats.OpenSlots > 0)
            {
               if (locations.HasComponent(target))
               {
                  LocalToWorld targetPos = locations[target];

                  float dist = math.abs(math.distance(transform.Position, targetPos.Position));
                  if (dist < bestTargetDist)
                  {
                     bestTargetDist = dist;
                     bestTarget = target;
                  }
               }
            }
         }
         if (bestTargetDist != math.INFINITY)
         {
            desires.workWeight = .5f;
            desires.workTarget = bestTarget;
         }
         else
         {
            desires.workWeight = -1;
         }
      }
   }

   [BurstCompile]
   public partial struct ProcessCleanerWorkSearch : IJobEntity
   {
      public NativeArray<Entity> messes;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;
      [ReadOnly]
      public ComponentLookup<DestinationCapicityData> capicatity;

      private void Execute(ref DestinationDesireData desires, in CleanerStateData _, in LocalToWorld transform)
      {
         float bestTargetDist = math.INFINITY;
         Entity bestTarget = default;

         foreach (Entity target in messes)
         {
            if (capicatity.TryGetComponent(target, out DestinationCapicityData seats) && seats.OpenSlots > 0)
            {
               if (locations.HasComponent(target))
               {
                  LocalToWorld targetPos = locations[target];

                  float dist = math.abs(math.distance(transform.Position, targetPos.Position));
                  if (dist < bestTargetDist)
                  {
                     bestTargetDist = dist;
                     bestTarget = target;
                  }
               }
            }
         }
         if (bestTargetDist != math.INFINITY)
         {
            desires.workWeight = .5f;
            desires.workTarget = bestTarget;
         }
         else
         {
            desires.workWeight = -1;
         }
      }
   }

}