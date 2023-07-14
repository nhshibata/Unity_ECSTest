using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using TriggerEvent = Unity.Physics.TriggerEvent;

/// <summary>
/// 複数の当たり判定のjobの発行を行う
/// 弾と敵のjobの発行も行う
/// </summary>
[BurstCompile]
public partial struct CollisionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // プレイヤーが存在していなければ処理しない
        state.RequireForUpdate<PlayerData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var player = state.GetComponentLookup<PlayerData>();
        var enemy = state.GetComponentLookup<EnemyData>();
        var item = state.GetComponentLookup<ItemData>();
        var bullet = state.GetComponentLookup<BulletData>();

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        float deltaTime = SystemAPI.Time.DeltaTime;

        // ジョブの発行
        // ItemとEnemy振る舞いの発行
        new ItemJob
        {
            deltaTime = deltaTime,
            ECB = ecb
        }.Schedule();
        
        new BulletJob
        {
            deltaTime = deltaTime,
            ECB = ecb
        }.Schedule();

        // 当たり判定ジョブの発行
        state.Dependency = new EnemyAndPlayerHitJob
        {
            enemys = enemy,
            players = player
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        //========================================================
        // particleシステムの取得
        //========================================================
        UnityEngine.ParticleSystem pss = null;
#if true
        // 取得方法1
        // エンティティのクエリングとコンポーネントの取得が明示的に分かれています。
        // エンティティクエリを作成し、エンティティの配列を取得してループで処理する必要があります。
        // コンポーネントを直接参照できるため、操作が直感的です。
        EntityQuery query = state.World.EntityManager.CreateEntityQuery(typeof(ParticleTag));
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
        foreach (Entity entity in entities)
        {
            // エンティティの処理
            pss = state.World.EntityManager.GetComponentObject<UnityEngine.ParticleSystem>(entity);
            break;
        }
        // 忘れずに解放
        entities.Dispose();
#else
        // 取得方法2
        // SystemAPI.Queryメソッドを使用することで、エンティティとコンポーネントを同時に取得できます。
        // WithAll<ParticleTag>()メソッドを使用して特定のタグを持つエンティティを絞り込むことができます。
        // ジョブシステムによる並列処理に適しています。
        foreach (var ps in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<UnityEngine.ParticleSystem>>().WithAll<ParticleTag>())
        {
            pss = ps.Value;
            break;
        }
#endif
        // エンティティの数が少なく、直感的な操作が必要な場合：方法1を使用
        // エンティティの数が多く、ジョブシステムによる並列処理が必要な場合：方法2を使用
        state.Dependency = new EnemyAndBulletHitJob
        {
            enemys = enemy,
            bullets = bullet,
            ECB = ecb,
            particleSystem = pss
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        //========================================================


        state.Dependency = new ItemAndPlayerHitJob
        {
            players = player,
            items = item
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        
        
    }
}


//==========================================================
// Job実装部
//==========================================================
[BurstCompile]
partial struct ItemJob : IJobEntity
{
    public float deltaTime;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(
        Entity entity,
        [ChunkIndexInQuery] int chunkIndexInQuery,
        ref ItemData item,
        ref LocalTransform localTransform)
    {
        localTransform.Rotation = localTransform.RotateY(deltaTime).Rotation;
        
        item.LimitTime -= deltaTime;
        if(item.LimitTime < 0)
        {

            ECB.DestroyEntity(chunkIndexInQuery, entity);
        }
    }
}

[BurstCompile]
partial struct BulletJob : IJobEntity
{
    public float deltaTime;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(
        Entity entity,
        [ChunkIndexInQuery] int chunkIndexInQuery,
        ref BulletData bullet,
        ref LocalTransform localTransform)
    {
        var acc = (localTransform.Forward() * bullet.Speed);
        localTransform.Position = localTransform.Position + acc;

        bullet.LimitTime -= deltaTime;
        if(bullet.LimitTime < 0)
        {
            ECB.DestroyEntity(chunkIndexInQuery, entity);
        }
    }
}


//==========================================================
// 当たり判定
// TODO:省略したい
//==========================================================

[BurstCompile]
[UpdateAfter(typeof(PhysicsSystemGroup))]
partial struct EnemyAndPlayerHitJob : ICollisionEventsJob
{
    public ComponentLookup<PlayerData> players;
    public ComponentLookup<EnemyData> enemys;

    [BurstCompile]
    public void Execute(CollisionEvent collisionEvent)
    {
        // Player==A
        // Enemy==B
        bool isPlayer = players.HasComponent(collisionEvent.EntityA);
        bool isEnemy = enemys.HasComponent(collisionEvent.EntityB);

        if(isPlayer && isEnemy)
        {
            HitEvent(collisionEvent.EntityA, collisionEvent.EntityB);
            // 接触時
            return;
        }

        isPlayer = players.HasComponent(collisionEvent.EntityB);
        isEnemy = enemys.HasComponent(collisionEvent.EntityA);
        if (isPlayer && isEnemy)
        {
            HitEvent(collisionEvent.EntityB, collisionEvent.EntityA);
        }

    }

    /// <summary>
    /// プレイヤーエネミー接触時処理
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="enemyID"></param>
    private void HitEvent(Entity playerID, Entity enemyID)
    {
        //players[playerID];
        //enemys[enemyID];
    }
}

[BurstCompile]
[UpdateAfter(typeof(PhysicsSystemGroup))]
partial struct EnemyAndBulletHitJob : ITriggerEventsJob
{
    public ComponentLookup<BulletData> bullets;
    public ComponentLookup<EnemyData> enemys;
    public EntityCommandBuffer.ParallelWriter ECB;
    public EntityManager entityMgr;
    public UnityEngine.ParticleSystem particleSystem;

    [BurstCompile]
    public void Execute(TriggerEvent collisionEvent)
    {
        // Player==A
        // Enemy==B
        bool isBullet = bullets.HasComponent(collisionEvent.EntityA);
        bool isEnemy = enemys.HasComponent(collisionEvent.EntityB);

        if (isBullet && isEnemy)
        {
            HitEvent(collisionEvent.EntityA, collisionEvent.EntityB);
            // 接触時
            return;
        }

        isBullet = bullets.HasComponent(collisionEvent.EntityB);
        isEnemy = enemys.HasComponent(collisionEvent.EntityA);
        if (isBullet && isEnemy)
        {
            HitEvent(collisionEvent.EntityB, collisionEvent.EntityA);
        }

    }

    /// <summary>
    /// プレイヤーエネミー接触時処理
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="enemyID"></param>
    private void HitEvent(Entity bulletID, Entity enemyID)
    {
        //bullets[playerID];
        //enemys[enemyID];
        
        ECB.DestroyEntity(bulletID.Index, bulletID);
        ECB.DestroyEntity(enemyID.Index, enemyID);

        // エフェクト再生
        particleSystem.Play();
    }

}

/// <summary>
/// アイテムとプレイヤー当たり判定
/// [UpdateAfter(typeof(PhysicsSystemGroup))]はPhysics処理後を指定する
/// </summary>
[BurstCompile]
[UpdateAfter(typeof(PhysicsSystemGroup))]
partial struct ItemAndPlayerHitJob : ITriggerEventsJob
{
    public ComponentLookup<PlayerData> players;
    public ComponentLookup<ItemData> items;

    public void Execute(TriggerEvent collisionEvent)
    {
        // Player==A
        // Item==B
        bool isPlayer = players.HasComponent(collisionEvent.EntityA);
        bool isItem = items.HasComponent(collisionEvent.EntityB);

        if (isPlayer && isItem)
        {
            // 接触時
            HitEvent(collisionEvent.EntityA, collisionEvent.EntityB);
            return;
        }

        isPlayer = players.HasComponent(collisionEvent.EntityB);
        isItem = items.HasComponent(collisionEvent.EntityA);
        if (isPlayer && isItem)
        {
            HitEvent(collisionEvent.EntityB, collisionEvent.EntityA);
        }
    }

    private void HitEvent(Entity playerID, Entity itemID)
    {

    }

}