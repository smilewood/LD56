using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ActivityData : IComponentData
{
   public float remainingTime;
   public Entity reservedSpotEntity;
   public DestinationCapicityData reservedSpot;

}


public class ActivityComponent : MonoBehaviour
{
}
