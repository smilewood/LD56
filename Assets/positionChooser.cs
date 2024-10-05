using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class positionChooser : MonoBehaviour
{
   public Vector3 targetPos;
   private MovableComponent target;
   // Start is called before the first frame update
   void Start()
   {
      //World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentObject(, )

      StartCoroutine(pickLocation());
   }

   private IEnumerator pickLocation()
   {
      while (true)
      {
         yield return new WaitForSeconds(5);
         targetPos = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
         target.Destination = targetPos;
      }
   }
}
