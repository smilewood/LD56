using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct DestinationCapicitySystem : ISystem
{

   [BurstCompile]
   public void OnUpdate(ref SystemState state)
   {
      EntityQuery activeGuyQuery = SystemAPI.QueryBuilder().WithAll<ActivityData>().Build();
      NativeArray<Entity> activeGuys = activeGuyQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.Handle);

      new UpdateCapicityJob
      {
         activity = SystemAPI.GetComponentLookup<ActivityData>(true),
         guys = activeGuys
      }.ScheduleParallel();
   }

   [BurstCompile]
   public partial struct UpdateCapicityJob : IJobEntity
   {
      [ReadOnly]
      public ComponentLookup<ActivityData> activity;
      public NativeArray<Entity> guys;

      private void Execute(ref DestinationCapicityData dest, Entity target)
      {
         int count = 0;
         foreach(Entity g in guys)
         {
            if (activity[g].reservedSpot.destEntity == target)
            {
               ++count;
            }
         }
         dest.CurrentOccupancy = count;
      }
   }
}
