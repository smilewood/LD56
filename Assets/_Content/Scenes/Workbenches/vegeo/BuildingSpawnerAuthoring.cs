using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BuildingSpawnerAuthoring : MonoBehaviour
{
    public GameObject FoodPrefab;
    public GameObject WaterPrefab;
    public GameObject MinePrefab;
    public GameObject BreadPrefab;
    public GameObject CompostPrefab;

    public class Baker : Baker<BuildingSpawnerAuthoring>
    {
        public override void Bake(BuildingSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingSpawner
            {
                FoodPrefab = GetEntity(authoring.FoodPrefab, TransformUsageFlags.Dynamic),
                WaterPrefab = GetEntity(authoring.WaterPrefab, TransformUsageFlags.Dynamic),
                MinePrefab = GetEntity(authoring.MinePrefab, TransformUsageFlags.Dynamic),
                BreadPrefab = GetEntity(authoring.BreadPrefab, TransformUsageFlags.Dynamic),
                CompostPrefab = GetEntity(authoring.CompostPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct BuildingSpawner : IComponentData
{
    public Entity FoodPrefab;
    public Entity WaterPrefab;
    public Entity MinePrefab;
    public Entity BreadPrefab;
    public Entity CompostPrefab;
}

public partial class BuildingSpawnerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<BuildingSpawner>();
    }


    protected override void OnUpdate()
    {
        if (BuildingSpawningManager.Instance == null)
        {
            return;
        }
        if (BuildingSpawningManager.Instance.currentBuildingSpawnType == BuildingSpawningManager.BuildingType.NOSPAWN)
        {
            return;
        }

        
        BuildingSpawner buildingSpawner = SystemAPI.GetSingleton<BuildingSpawner>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        switch (BuildingSpawningManager.Instance.currentBuildingSpawnType)
        {
            case BuildingSpawningManager.BuildingType.Food:
                SpawnBuilding(buildingSpawner.FoodPrefab, ecb);
                break;
            case BuildingSpawningManager.BuildingType.Water:
                SpawnBuilding(buildingSpawner.WaterPrefab, ecb);
                break;
            case BuildingSpawningManager.BuildingType.Mine:
                SpawnBuilding(buildingSpawner.MinePrefab, ecb);
                break;
            case BuildingSpawningManager.BuildingType.Bread:
                SpawnBuilding(buildingSpawner.BreadPrefab, ecb);
                break;
            case BuildingSpawningManager.BuildingType.Compost:
                SpawnBuilding(buildingSpawner.CompostPrefab, ecb);
                break;
        }
    }

    private void SpawnBuilding(Entity prefab, EntityCommandBuffer ecb)
    {
        Entity entity = ecb.Instantiate(prefab);
        ecb.SetComponent(entity, new LocalTransform { Position = BuildingSpawningManager.Instance.spawnPosition, Rotation = BuildingSpawningManager.Instance.spawnRotation, Scale = 1f });
        BuildingSpawningManager.Instance.currentBuildingSpawnType = BuildingSpawningManager.BuildingType.NOSPAWN;

        ecb.Playback(EntityManager);
    }
}