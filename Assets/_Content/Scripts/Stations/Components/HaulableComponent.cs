using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public enum ResourceType
{
   Bread, Ore, Biomass
}

public struct HaulableData : IComponentData
{
   public ResourceType Type;
}

public class HaulableComponent : MonoBehaviour
{
   public ResourceType ResourceType;

   public class HaulableBaker : Baker<HaulableComponent>
   {
      public override void Bake(HaulableComponent authoring)
      {
         Entity target = GetEntity(TransformUsageFlags.Dynamic);

         AddComponent(target, new HaulableData { Type = authoring.ResourceType });
      }
   }
}


