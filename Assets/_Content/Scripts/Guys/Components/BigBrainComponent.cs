using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct DestinationDesireData : IComponentData
{
   public float foodWeight;
   public Entity foodTarget;

   public float  waterWeight;
   public Entity waterTarget;

   public float  workWeight;
   public Entity workTarget;

}

public struct CurrentDestinationData : IComponentData
{
   public float ApproachRadius;
   public Entity destination;
   public float3 destinationLocation;
}

public class BigBrainComponent : MonoBehaviour
{

}

public class BigBrainBaker : Baker<BigBrainComponent>
{
   public override void Bake(BigBrainComponent authoring)
   {
      Entity target = GetEntity(TransformUsageFlags.None);
      
      AddComponent(target, new DestinationDesireData { });
   }
}