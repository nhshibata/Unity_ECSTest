using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// GitHubのEntityComponentSystemSamplesを参考に作成
/// SpawSettingsを生成する
/// </summary>
public partial class SpawnCubeSystem : SpawnRandomObjectsSystem<SpawnSettings>
{
    protected override void OnCreate()
    {
        UnityEngine.Debug.Log("ランダム生成???");
    }
}

/// <summary>
/// 生成システム
/// ジェネリック型(IComponentDataであることを保障)
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class SpawnRandomObjectsSystem<T> : SystemBase where T : unmanaged, IComponentData, ISpawnSettings
{
    [BurstCompile]
    protected override void OnUpdate()
    {
        using(var entities = GetEntityQuery(new ComponentType[] { typeof(T) }).ToEntityArray(Allocator.TempJob))
        {
            for(int j = 0; j < entities.Length; ++j)
            {
                var entity = entities[j];
                var spawnSettings = EntityManager.GetComponentData<T>(entity);

#if UNITY_ANDROID || UNITY_IOS
                    // Limit the number of bodies on platforms with potentially low-end devices
                    var count = math.min(spawnSettings.Count, 500);
#else
                var count = spawnSettings.Count;
#endif

                //OnBeforeInstantiatePrefab(ref spawnSettings);

                var instances = new NativeArray<Entity>(count, Allocator.Temp);
                EntityManager.Instantiate(spawnSettings.Prefab, instances);

                var positions = new NativeArray<float3>(count, Allocator.Temp);
                var rotations = new NativeArray<quaternion>(count, Allocator.Temp);
                RandomPointsInRange(spawnSettings.Position, spawnSettings.Rotation, spawnSettings.Range, ref positions, ref rotations, GetRandomSeed(spawnSettings));

                for (int i = 0; i < count; i++)
                {
                    var instance = instances[i];

                    var transform = EntityManager.GetComponentData<LocalTransform>(instance);
                    transform.Position = positions[i];
                    transform.Rotation = rotations[i];
                    EntityManager.SetComponentData(instance, transform);

                    //ConfigureInstance(instance, ref spawnSettings);
                }

                EntityManager.RemoveComponent<T>(entity);
            }
        }

    }

    protected static void RandomPointsInRange(
         float3 center, quaternion orientation, float3 range,
         ref NativeArray<float3> positions, ref NativeArray<quaternion> rotations, int seed = 0)
    {
        var count = positions.Length;

        var random = Random.CreateFromIndex((uint)seed);
        for (int i = 0; i < count; i++)
        {
            positions[i] = center + math.mul(orientation, random.NextFloat3(-range, range));
            rotations[i] = math.mul(random.NextQuaternionRotation(), orientation);
        }
    }

    internal virtual int GetRandomSeed(T spawnSettings)
    {
        var seed = spawnSettings.RandomSeedOffset;
        seed = (seed * 397) ^ spawnSettings.Count;
        seed = (seed * 397) ^ (int)math.csum(spawnSettings.Position);
        seed = (seed * 397) ^ (int)math.csum(spawnSettings.Range);
        return seed;
    }

}