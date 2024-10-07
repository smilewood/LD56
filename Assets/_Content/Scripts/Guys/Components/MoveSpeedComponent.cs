using Unity.Entities;
using UnityEngine;
public struct MoveSpeedData : IComponentData
{
   public float MoveSpeed;
   public float MoveMult;
}

public class MoveSpeedComponent : MonoBehaviour
{
   public float speed;
}

public class MoveSpeedBaker : Baker<MoveSpeedComponent>
{
   public override void Bake(MoveSpeedComponent authoring)
   {
      Entity target = GetEntity(TransformUsageFlags.None);
      AddComponent(target, new MoveSpeedData
      {
         MoveSpeed = authoring.speed + Random.Range(-.5f, .5f),
         MoveMult = 1
      });
   }
}
