using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class SpawnCreatureConfigAuthoring : MonoBehaviour
{
    public GameObject CreaturePrefab;
    public int amountToSpawn;
    public float SpawnInterval;

    public class Baker : Baker<SpawnCreatureConfigAuthoring>
    {
        public override void Bake(SpawnCreatureConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawnCreatureConfig
            {
                CreaturePrefab = GetEntity(authoring.CreaturePrefab, TransformUsageFlags.Dynamic),
                amountToSpawn = authoring.amountToSpawn,
                SpawnInterval = authoring.SpawnInterval
            });
        }
    }
}

public struct SpawnCreatureConfig : IComponentData
{
    public Entity CreaturePrefab;
    public int amountToSpawn;
    public float SpawnInterval;
}

public partial class SpawnCreatureSystem : SystemBase
{
    private float _lastSpawn;

    protected override void OnCreate()
    {
        RequireForUpdate<SpawnCreatureConfig>();
    }

    protected override void OnUpdate()
    {
        SpawnCreatureConfig spawnCreatureConfig = SystemAPI.GetSingleton<SpawnCreatureConfig>();

        if (SystemAPI.Time.ElapsedTime - _lastSpawn > spawnCreatureConfig.SpawnInterval)
        {
            _lastSpawn = (float)SystemAPI.Time.ElapsedTime;

            for (int i = 0; i < spawnCreatureConfig.amountToSpawn; i++)
                Spawn(spawnCreatureConfig);
        }
    }

    private void Spawn(SpawnCreatureConfig spawnCreatureConfig)
    {
        Entity entity = EntityManager.Instantiate(spawnCreatureConfig.CreaturePrefab);
        SystemAPI.SetComponent(entity, new LocalTransform
        {
            Position = new float3(UnityEngine.Random.Range(-10f, 10f), 1f, UnityEngine.Random.Range(-10f, 10f)),
            Rotation = quaternion.identity,
            Scale = 1f
        });
    }
}
