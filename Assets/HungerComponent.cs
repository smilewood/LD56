using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct HaveHungerData : IComponentData
{
   public float MaxHunger;
   public float CurrentHunger;
   public float DrainPerSec;
   public float HungerDrainMult;
}

public class HaveHungerComponent : MonoBehaviour
{
   public float StomachSize;
   public float DrainPerSec;
}

public class HaveHungerBaker : Baker<HaveHungerComponent>
{
   public override void Bake(HaveHungerComponent authoring)
   {
      Entity target = GetEntity(TransformUsageFlags.None);

      AddComponent(target, new HaveHungerData
      {
         MaxHunger = authoring.StomachSize,
         CurrentHunger = authoring.StomachSize,
         DrainPerSec = authoring.DrainPerSec,
         HungerDrainMult = 1
      });
   }
}