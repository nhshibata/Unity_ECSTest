using Unity.Burst;
using Unity.Transforms;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// partialにしているのはUnity側で自動生成される処理をこのクラスに適用するために必須です
/// </summary>
[BurstCompile]
public partial struct SpawnerSystem : ISystem
{ 
    public void OnCreate(ref SystemState system)
    {
    }
    public void OnDestroy(ref SystemState system)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // すべてのSpawnerコンポーネントをクエリします。このシステムは、
        // コンポーネントから読み取りと書き込みを行う必要があるため、RefRWを使用します。
        // システムが読み取り専用のアクセスのみを必要とする場合は、RefROを使用します。
        foreach (RefRW<SpawnComponent> spawner in SystemAPI.Query<RefRW<SpawnComponent>>())
        {
            ProcessSpawner(ref state, spawner);
        }
    }

    private void ProcessSpawner(ref SystemState state, RefRW<SpawnComponent> spawner)
    {
        // 次のスポーン時間が経過している場合    
        if (spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime)
        {
            // スポナーの持つプレファブを使って、新しいエンティティを生成します。
            Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
            // LocalPosition.FromPositionは、指定された位置で初期化されたTransformを返します。
            state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition));

            // 次のスポーン時間をリセットします。
            spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
        }
    }
}
