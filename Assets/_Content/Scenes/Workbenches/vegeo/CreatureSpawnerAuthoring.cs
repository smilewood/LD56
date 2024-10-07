using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CreatureSpawnerAuthoring : MonoBehaviour
{
    public GameObject CleanerPrefab;
    public GameObject HaulerPrefab;
    public GameObject ProducerPrefab;
    public Vector3 spawnPosition;

    public class Baker : Baker<CreatureSpawnerAuthoring>
    {
        public override void Bake(CreatureSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new CreatureSpawner
            {
                CleanerPrefab = GetEntity(authoring.CleanerPrefab, TransformUsageFlags.Dynamic),
                HaulerPrefab = GetEntity(authoring.HaulerPrefab, TransformUsageFlags.Dynamic),
                ProducerPrefab = GetEntity(authoring.ProducerPrefab, TransformUsageFlags.Dynamic),
                spawnPosition = authoring.spawnPosition
            });
        }
    }
}

public struct CreatureSpawner : IComponentData
{
    public Entity CleanerPrefab;
    public Entity HaulerPrefab;
    public Entity ProducerPrefab;
    public Vector3 spawnPosition;
}

public partial class CreatureSpawnerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<CreatureSpawner>();
    }

    protected override void OnUpdate()
    {
        if (CreatureSpawnManager.Instance == null)
        {
            return;
        }
        if (CreatureSpawnManager.Instance.CleanerCount == 0 && CreatureSpawnManager.Instance.HaulerCount == 0 && CreatureSpawnManager.Instance.ProducerCount == 0)
        {
            return;
        }

        CreatureSpawner creatureSpawner = SystemAPI.GetSingleton<CreatureSpawner>();

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        for (int i = 0; i < CreatureSpawnManager.Instance.CleanerCount; i++)
        {
            Entity entity = ecb.Instantiate(creatureSpawner.CleanerPrefab);
            ecb.SetComponent(entity, new LocalTransform { Position = creatureSpawner.spawnPosition, Rotation = quaternion.identity, Scale = 1f });
        }
        CreatureSpawnManager.Instance.CleanerCount = 0;

        for (int i = 0; i < CreatureSpawnManager.Instance.HaulerCount; i++)
        {
            Entity entity = ecb.Instantiate(creatureSpawner.HaulerPrefab);
            ecb.SetComponent(entity, new LocalTransform { Position = creatureSpawner.spawnPosition, Rotation = quaternion.identity, Scale = 1f });
        }
        CreatureSpawnManager.Instance.HaulerCount = 0;

        for (int i = 0; i < CreatureSpawnManager.Instance.ProducerCount; i++)
        {
            Entity entity = ecb.Instantiate(creatureSpawner.ProducerPrefab);
            ecb.SetComponent(entity, new LocalTransform { Position = creatureSpawner.spawnPosition, Rotation = quaternion.identity, Scale = 1f });
        }
        CreatureSpawnManager.Instance.ProducerCount = 0;

        ecb.Playback(EntityManager);
    }
}