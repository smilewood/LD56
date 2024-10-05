using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct DestinationDesireData : IBufferElementData
{
   public float weight;
   public float3 target;
}

public struct CurrentDestinationData : IComponentData
{
   public float3 destination;
}



public class BigBrainComponent : MonoBehaviour
{

}

public class BigBrainBaker : Baker<BigBrainComponent>
{
   public override void Bake(BigBrainComponent authoring)
   {
      Entity target = GetEntity(TransformUsageFlags.None);

      AddBuffer<DestinationDesireData>(target);

      AddComponent(target, new CurrentDestinationData { });
   }
}