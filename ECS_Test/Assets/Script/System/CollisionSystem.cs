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
/// �����̓����蔻���job�̔��s���s��
/// �e�ƓG��job�̔��s���s��
/// </summary>
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
        // Item��Enemy�U�镑���̔��s
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

        //========================================================
        // particle�V�X�e���̎擾
        //========================================================
        UnityEngine.ParticleSystem pss = null;
#if true
        // �擾���@1
        // �G���e�B�e�B�̃N�G�����O�ƃR���|�[�l���g�̎擾�������I�ɕ�����Ă��܂��B
        // �G���e�B�e�B�N�G�����쐬���A�G���e�B�e�B�̔z����擾���ă��[�v�ŏ�������K�v������܂��B
        // �R���|�[�l���g�𒼐ڎQ�Ƃł��邽�߁A���삪�����I�ł��B
        EntityQuery query = state.World.EntityManager.CreateEntityQuery(typeof(ParticleTag));
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
        foreach (Entity entity in entities)
        {
            // �G���e�B�e�B�̏���
            pss = state.World.EntityManager.GetComponentObject<UnityEngine.ParticleSystem>(entity);
            break;
        }
        // �Y�ꂸ�ɉ��
        entities.Dispose();
#else
        // �擾���@2
        // SystemAPI.Query���\�b�h���g�p���邱�ƂŁA�G���e�B�e�B�ƃR���|�[�l���g�𓯎��Ɏ擾�ł��܂��B
        // WithAll<ParticleTag>()���\�b�h���g�p���ē���̃^�O�����G���e�B�e�B���i�荞�ނ��Ƃ��ł��܂��B
        // �W���u�V�X�e���ɂ����񏈗��ɓK���Ă��܂��B
        foreach (var ps in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<UnityEngine.ParticleSystem>>().WithAll<ParticleTag>())
        {
            pss = ps.Value;
            break;
        }
#endif
        // �G���e�B�e�B�̐������Ȃ��A�����I�ȑ��삪�K�v�ȏꍇ�F���@1���g�p
        // �G���e�B�e�B�̐��������A�W���u�V�X�e���ɂ����񏈗����K�v�ȏꍇ�F���@2���g�p
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
        
        ECB.DestroyEntity(bulletID.Index, bulletID);
        ECB.DestroyEntity(enemyID.Index, enemyID);

        // �G�t�F�N�g�Đ�
        particleSystem.Play();
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
            // �ڐG��
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