using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct CleaningSpotData : IComponentData
{
   public float CleaningDistance;
   public float CleaningTime;
}


public class CleanSpotComponent : MonoBehaviour
{
   public float CleaningDistance;
   public float CleaningTime;

   public class CleanSpotBaker : Baker<CleanSpotComponent>
   {
      public override void Bake(CleanSpotComponent authoring)
      {
         Entity target = GetEntity(TransformUsageFlags.Dynamic);

         AddComponent(target, new CleaningSpotData { CleaningDistance = authoring.CleaningDistance, CleaningTime = authoring.CleaningTime });
         AddComponent(target, new DestinationCapicityData
         {
            destEntity = target,
            MaxOccupency = 1
         });
      }
   }
}
