using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor.Rendering;
using UnityEngine;

public enum WorkerType
{
   Producer, Hauler, Cleaner
}

public struct WorkerTypeData : IComponentData
{
   public WorkerType WorkerType;
}

public class WorkerTypeComponent : MonoBehaviour
{
   public WorkerType TypeOfWorker;

   public class WorkerTypeBaker : Baker<WorkerTypeComponent>
   {
      public override void Bake(WorkerTypeComponent authoring)
      {
         Entity target = GetEntity(TransformUsageFlags.Dynamic);
         AddComponent(target, new WorkerTypeData { WorkerType = authoring.TypeOfWorker });
      }
   }
}
