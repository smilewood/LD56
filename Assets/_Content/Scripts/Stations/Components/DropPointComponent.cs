using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct DropPointData : IComponentData
{
   
}

public class DropPointComponent : MonoBehaviour
{

   public class DropPointBaker : Baker<DropPointComponent>
   {
      public override void Bake(DropPointComponent authoring)
      {
         Entity target = GetEntity(TransformUsageFlags.None);

         AddComponent(target, new DropPointData { });
      }
   }
}