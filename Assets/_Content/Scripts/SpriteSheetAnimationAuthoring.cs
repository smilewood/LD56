using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class SpriteSheetAnimationAuthoring : MonoBehaviour
{
    public Color color;
    public int creatureIndex;
    public int animationIndex;
    public int animationCount;
    public int creatureCount;
    public int frameCount;
    public float fps;

    private class Baker : Baker<SpriteSheetAnimationAuthoring>
    {
        public override void Bake(SpriteSheetAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpriteSheetAnimation
            {
                color = authoring.color,
                frameCount = authoring.frameCount,
                fps = authoring.fps,
                animationIndex = authoring.animationIndex,
                animationCount = authoring.animationCount,
                creatureCount = authoring.creatureCount,
                creatureIndex = authoring.creatureIndex
            });
        }
    }
}

public struct SpriteSheetAnimation : IComponentData
{
    public int creatureIndex;
    public int animationIndex;

    public Color color;

    public float animationTime;
    public int frameCount;
    public int animationCount;
    public int creatureCount;
    public float fps;

    public Vector4 uv;
    public Matrix4x4 matrix;
}

public partial struct SpriteSheetAnimationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpriteSheetAnimation>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        AnimationJob animationJob = new AnimationJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        animationJob.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct AnimationJob : IJobEntity
    {
        public float deltaTime;

        public void Execute(ref SpriteSheetAnimation spriteSheetAnimation, in LocalToWorld localToWorld)
        {
            spriteSheetAnimation.animationTime += deltaTime * spriteSheetAnimation.fps;
            spriteSheetAnimation.animationTime %= spriteSheetAnimation.frameCount;

            float uvWidth = 1f / spriteSheetAnimation.frameCount;
            float uvHeight = 1f / (spriteSheetAnimation.animationCount * spriteSheetAnimation.creatureCount);
            float uvOffsetX = uvWidth * (int)spriteSheetAnimation.animationTime;
            float uvOffsetY = uvHeight * spriteSheetAnimation.animationIndex + (uvHeight * spriteSheetAnimation.animationCount * spriteSheetAnimation.creatureIndex);
            spriteSheetAnimation.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);

            spriteSheetAnimation.matrix = Matrix4x4.TRS(localToWorld.Position + new float3(0, 0.5f,0), Quaternion.identity, Vector3.one);
        }
    }
}

[UpdateAfter(typeof(SpriteSheetAnimationSystem))]
public partial struct SpriteSheetRendererSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpriteSheetAnimation>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityQuery query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<SpriteSheetAnimation>());
        NativeArray<SpriteSheetAnimation> animationArray = query.ToComponentDataArray<SpriteSheetAnimation>(Allocator.TempJob);

        List<Matrix4x4> matrices = new List<Matrix4x4>();
        List<Vector4> uvs = new List<Vector4>();
        List<Vector4> colors = new List<Vector4>();
        for (int i = 0; i < animationArray.Length; i++)
        {
            SpriteSheetAnimation spriteSheetAnimation = animationArray[i];
            matrices.Add(spriteSheetAnimation.matrix);
            uvs.Add(spriteSheetAnimation.uv);
            colors.Add(spriteSheetAnimation.color);
        }


        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        Vector4[] uv = new Vector4[1];
        Camera camera = Camera.main;
        Mesh mesh = SpriteSheetRendererProvider.Instance.QuadMesh;
        Material material = SpriteSheetRendererProvider.Instance.CreatureMaterial;
        int shaderPropertyID = Shader.PropertyToID("_MainTex_UV");

        materialPropertyBlock.SetVectorArray(shaderPropertyID, uvs);
        materialPropertyBlock.SetVectorArray("_Color", colors);

        Graphics.DrawMeshInstanced(
            mesh,
            0,
            material,
            matrices,
            materialPropertyBlock
        );
    }
}