using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor.Rendering;
using UnityEngine;

public enum WorkerType
{
   Producer, Hauler, Cleaner
}

public struct ProducerStateData : IComponentData
{
}

public struct HaulerStateData : IComponentData
{
   public bool Hauling;
   public ResourceType TypeBeingHauled;
}

public struct CleanerStateData : IComponentData
{
}


public class WorkerTypeComponent : MonoBehaviour
{
   public WorkerType TypeOfWorker;

   public class WorkerTypeBaker : Baker<WorkerTypeComponent>
   {
      public override void Bake(WorkerTypeComponent authoring)
      {
         Entity target = GetEntity(TransformUsageFlags.Dynamic);
         switch (authoring.TypeOfWorker)
         {
            case WorkerType.Producer:
               AddComponent(target, new ProducerStateData { });
            break;
            case WorkerType.Hauler:
               AddComponent(target, new HaulerStateData { });
            break;
            case WorkerType.Cleaner:
               AddComponent(target, new CleanerStateData { });
            break;
         }
      }
   }
}
