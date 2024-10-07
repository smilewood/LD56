using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct MakesAMessData : IComponentData
{
   public float BaseTime, RemainingTime;
   public Entity MessEntity;
}

public class MakesAMessComponent : MonoBehaviour
{
   public float Frequency;
   public GameObject MessPrefab;

   public class MakesAMessBaker : Baker<MakesAMessComponent>
   {
      public override void Bake(MakesAMessComponent authoring)
      {
         Entity target = GetEntity(TransformUsageFlags.Dynamic);
         AddComponent(target, new MakesAMessData
         {
            BaseTime = authoring.Frequency,
            RemainingTime = authoring.Frequency,
            MessEntity = GetEntity(authoring.MessPrefab, TransformUsageFlags.Dynamic)
         });
      }
   }
}


