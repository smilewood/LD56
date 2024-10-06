using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public enum ModifierType
{
   Hunger,
   Thirst,
   Health,
   Happiness
}

public struct ModifierData : IBufferElementData
{
   public ModifierType ModType;
   public float MaxValue;
   public float CurrentValue;
   public float naturalDecayRate;
   public float modifier;
}

public struct CanHaveModifiers : IComponentData
{

}

[System.Serializable]
public struct ModifierDefault
{
   public ModifierType ModType;
   public float startValue;
   public float naturalDecayRate;
}

public class ModifierComponent : MonoBehaviour
{
   public List<ModifierDefault> baselines;

   public class ModifierBaker : Baker<ModifierComponent>
   {
      public override void Bake(ModifierComponent authoring)
      {
         Entity target = GetEntity(TransformUsageFlags.None);
         AddComponent(target, new CanHaveModifiers { });
         AddBuffer<ModifierData>(target);
         if (authoring.baselines != null)
         {
            foreach (ModifierDefault mod in authoring.baselines)
            {
               AppendToBuffer(target, new ModifierData
               {
                  ModType = mod.ModType,
                  MaxValue = mod.startValue,
                  CurrentValue = mod.startValue,
                  naturalDecayRate = mod.naturalDecayRate,
                  modifier = 1
               });
            }
         }
      }
   }
}


