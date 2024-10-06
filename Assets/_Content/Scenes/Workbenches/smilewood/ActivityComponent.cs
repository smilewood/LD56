using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public enum ActivityType
{
   Eat, Drink, Produce, Clean
}

public struct ActivityData : IComponentData
{
   public float remainingTime;
   public DestinationCapicityData reservedSpot;
   public ActivityType Activity;
   public Entity ActivityTarget;
}


public class ActivityComponent : MonoBehaviour
{
}
