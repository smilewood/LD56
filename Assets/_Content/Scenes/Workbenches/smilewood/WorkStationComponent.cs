using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct WorkStationData : IComponentData
{
   public float DrinkDistance;
   public float DrinkTime;
   public Entity WorkResultPrefab;
}


public class WorkStationComponent : MonoBehaviour
{
   public GameObject WorkResultPrefab;
   public float MaxWorking;
   public float WorkDistance;
   public float WorkTime;
}

public class WorkStationBaker : Baker<WorkStationComponent>
{
   public override void Bake(WorkStationComponent authoring)
   {
      Entity target = GetEntity(TransformUsageFlags.Dynamic);

      AddComponent(target, new WorkStationData
      {
         DrinkDistance = authoring.WorkDistance,
         DrinkTime = authoring.WorkTime,
         WorkResultPrefab = GetEntity(authoring.WorkResultPrefab, TransformUsageFlags.None)
      });

      AddComponent(target, new DestinationCapicityData
      {
         MaxOccupency = authoring.MaxWorking,
         CurrentOccupancy = 0
      });

      //AddComponentObject(target, authoring.gameObject.GetComponent<FoodStationActions>());
   }
}
