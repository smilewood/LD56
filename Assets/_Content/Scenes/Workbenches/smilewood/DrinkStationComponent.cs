using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct DrinkStationData : IComponentData
{
   public float DrinkDistance;
   public float DrinkTime;
}


public class DrinkStationComponent : MonoBehaviour
{
   public float MaxDrinking;
   public float DrinkDistance;
   public float drinkTime;
}

public class DrinkStationBaker : Baker<DrinkStationComponent>
{
   public override void Bake(DrinkStationComponent authoring)
   {
      Entity target = GetEntity(TransformUsageFlags.Dynamic);

      AddComponent(target, new WorkStationData
      {
         DrinkDistance = authoring.DrinkDistance,
         DrinkTime = authoring.drinkTime,
      });

      AddComponent(target, new DestinationCapicityData
      {
         MaxOccupency = authoring.MaxDrinking,
         CurrentOccupancy = 0
      });

      //AddComponentObject(target, authoring.gameObject.GetComponent<FoodStationActions>());
   }
}
