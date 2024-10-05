using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct MovableData : IComponentData
{
   public float3? Destination;
   public float speed;
}

public class MovableComponent : MonoBehaviour
{
   public float speed;
}

public class MovableBaker : Baker<MovableComponent>
{
   public override void Bake(MovableComponent authoring)
   {
      Entity myEntitiy = GetEntity(TransformUsageFlags.None);

      AddComponent(myEntitiy, new MovableData
      {
         Destination = null,
         speed = authoring.speed
      });
   }
}