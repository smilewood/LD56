using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class TestCreatureAuthoring : MonoBehaviour
{
    public float Speed;

    private class Baker : Baker<TestCreatureAuthoring>
    {
        public override void Bake(TestCreatureAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new TestCreature {
                Speed = authoring.Speed,
                TargetPosition = float3.zero
            });
        }
    }
}

public struct TestCreature : IComponentData
{
    public float Speed;
    public float3 TargetPosition;
}

public partial struct TestCreatureSystem : ISystem
{
    private NativeArray<Unity.Mathematics.Random> _randoms;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TestCreature>();

        _randoms = new NativeArray<Unity.Mathematics.Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        uint r = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
        {
            _randoms[i] = new Unity.Mathematics.Random(r == 0 ? r + 1 : r);
        }
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {


        CreatureJob creatureJob = new CreatureJob
        {
            targetPosition = new Vector3(0, 0, 20),
            Randoms = _randoms,
            deltaTime = SystemAPI.Time.DeltaTime
        };
        creatureJob.ScheduleParallel();

    }

    [BurstCompile]
    public partial struct CreatureJob : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Unity.Mathematics.Random> Randoms;
        public float deltaTime;

        public Vector3 targetPosition;

        [NativeSetThreadIndex] private int _threadId;

        public void Execute(ref PhysicsVelocity physicsVelocity, ref TestCreature creature, ref LocalTransform localTransform, in LocalToWorld localToWorld)
        {
            Unity.Mathematics.Random random = Randoms[_threadId];

            localTransform.Rotation = quaternion.identity;
            creature.TargetPosition = targetPosition;
            physicsVelocity.Linear += deltaTime * creature.Speed * math.normalizesafe(creature.TargetPosition - localToWorld.Position);

            Randoms[_threadId] = random;
        }
    }
}
