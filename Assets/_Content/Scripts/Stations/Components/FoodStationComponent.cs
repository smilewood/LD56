using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

public struct FoodStationData : IComponentData
{
   public float FeedingDistance;
   public float EatingTime;
}

public struct DestinationCapicityData : IComponentData
{
   public float MaxOccupency;
   public float CurrentOccupancy;
   public Entity destEntity;
   public float OpenSlots
   {
      get
      {
         return MaxOccupency - CurrentOccupancy;
      }
   }
}

public class FoodStationComponent : MonoBehaviour
{
   public float MaxFeeding;
   public float FeedingDistance;
   public float EatingTime;
}

public class FoodStationBaker : Baker<FoodStationComponent>
{
   public override void Bake(FoodStationComponent authoring)
   {
      Entity target = GetEntity(TransformUsageFlags.Dynamic);

      AddComponent(target, new FoodStationData
      {
         FeedingDistance = authoring.FeedingDistance,
         EatingTime = authoring.EatingTime
      });

      AddComponent(target, new DestinationCapicityData
      {
         MaxOccupency = authoring.MaxFeeding,
         CurrentOccupancy = 0,
         destEntity = target
      });
      
      AddComponentObject(target, authoring.gameObject.GetComponent<FoodStationActions>());
   }
}
