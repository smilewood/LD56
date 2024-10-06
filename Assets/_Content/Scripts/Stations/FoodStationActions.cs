using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkstationActions : MonoBehaviour
{
   public abstract void Test();
}

public class FoodStationActions : WorkstationActions
{
   public override void Test()
   {
      Debug.Log($"Feeding is happening at {this.gameObject.name}");
   }
}
