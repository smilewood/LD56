using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawnManager : MonoBehaviour
{
    public static CreatureSpawnManager Instance { get; private set; }

    public int CleanerCount;
    public int HaulerCount;
    public int ProducerCount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}
