using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using TriggerEvent = Unity.Physics.TriggerEvent;

[BurstCompile]
public partial struct CollisionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // �v���C���[�����݂��Ă��Ȃ���Ώ������Ȃ�
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

        // �W���u�̔��s
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

        // �����蔻��W���u�̔��s
        state.Dependency = new EnemyAndPlayerHitJob
        {
            enemys = enemy,
            players = player
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        state.Dependency = new EnemyAndBulletHitJob
        {
            enemys = enemy,
            bullets = bullet
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        state.Dependency = new ItemAndPlayerHitJob
        {
            players = player,
            items = item
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}


//==========================================================
// Job������
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
// �����蔻��
// TODO:�ȗ�������
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
            // �ڐG��
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
    /// �v���C���[�G�l�~�[�ڐG������
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
    //ComponentLookup<Bull> enemys;

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
            // �ڐG��
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
    /// �v���C���[�G�l�~�[�ڐG������
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="enemyID"></param>
    private void HitEvent(Entity bulletID, Entity enemyID)
    {
        //bullets[playerID];
        //enemys[enemyID];
    }
}

/// <summary>
/// �A�C�e���ƃv���C���[�����蔻��
/// [UpdateAfter(typeof(PhysicsSystemGroup))]��Physics��������w�肷��
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
            HitEvent(collisionEvent.EntityA, collisionEvent.EntityB);
            // �ڐG��
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