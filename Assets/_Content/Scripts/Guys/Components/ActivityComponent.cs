using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum ActivityType
{
   Eat, Drink, Produce, Clean
}

public struct ActivityData : IComponentData
{
   public float remainingTime;
   public float3 Location;
   public DestinationCapicityData reservedSpot;
   public ActivityType Activity;
   public Entity ActivityTarget;
}


public class ActivityComponent : MonoBehaviour
{
}
