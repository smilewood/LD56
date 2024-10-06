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

      new ProcessWorkFinder
      {
         ecb = ecb,
         sources = workStationLocations,
         locations = SystemAPI.GetComponentLookup<LocalToWorld>(),
         capicatity = SystemAPI.GetComponentLookup<DestinationCapicityData>()
      }.ScheduleParallel();

   }

   [BurstCompile]
   public partial struct ProcessWorkFinder : IJobEntity
   {
      public EntityCommandBuffer.ParallelWriter ecb;
      public NativeArray<Entity> sources;
      [ReadOnly]
      public ComponentLookup<LocalToWorld> locations;
      [ReadOnly]
      public ComponentLookup<DestinationCapicityData> capicatity;

      private void Execute(ref DestinationDesireData desires, in WorkerTypeData workerType, in LocalToWorld transform)
      {
         float bestTargetDist = math.INFINITY;
         Entity bestTarget = default;

         foreach (Entity target in sources)
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