using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

public class FoodStationData : IComponentData
{
   public float FeedingDistance;
   public GameObject StationGO;
   public float MaxOccupency;
   public float CurrentOccupancy;
}

public class FoodStationComponent : MonoBehaviour
{
   public float MaxFeeding;
   public float FeedingDistance;
}

public class FoodStationBaker : Baker<FoodStationComponent>
{
   public override void Bake(FoodStationComponent authoring)
   {
      Entity target = GetEntity(TransformUsageFlags.Dynamic);

      AddComponentObject(target, new FoodStationData
      {
         FeedingDistance = authoring.FeedingDistance,
         MaxOccupency = authoring.MaxFeeding,
         CurrentOccupancy = 0,
         StationGO = authoring.gameObject
      });
   }
}
