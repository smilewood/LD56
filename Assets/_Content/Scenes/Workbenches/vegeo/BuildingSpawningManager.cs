using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BuildingSpawningManager : MonoBehaviour
{
    public static BuildingSpawningManager Instance;

    public BuildingType currentBuildingSpawnType;
    public Vector3 spawnPosition;
    public quaternion spawnRotation;

    private void Awake()
    {
        Instance = this;
    }

    public enum BuildingType
    {
        NOSPAWN,
        Food,
        Water,
        Compost,
        Mine,
        Bread,
        Omnicore
    }
}
