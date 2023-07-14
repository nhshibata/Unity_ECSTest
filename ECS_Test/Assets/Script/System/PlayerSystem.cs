using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Playerのシステム部分
/// ISystemでは参照を持つことが出来ない
/// </summary>
[BurstCompile]
[RequireMatchingQueriesForUpdate]
public partial class PlayerSystem : SystemBase
{
    readonly float JUMP_FORCE = 100.0f;

    InputAction move;
    InputAction jump;
    Asset.PlayerInputAction playerInput;
    bool isTryJumped;

    protected override void OnCreate()
    {
        // 存在しなければ処理しない
        base.RequireForUpdate<CharacterGunInput>();

        // 入力の受付を取得
        playerInput = new Asset.PlayerInputAction();
        playerInput.Enable();

        // 参照を保存
        move = playerInput.Player.Move;
        jump = playerInput.Player.Jump;
    }

    protected override void OnDestroy()
    {
        playerInput.Disable();
        playerInput.Dispose();
    }

    //[BurstCompile]
    protected override void OnUpdate()
    {
        Vector2 moveValue = move.ReadValue<Vector2>();

        // 使用変数
        var deltaTime = UnityEngine.Time.deltaTime;
        var tickTime = 1f / deltaTime;
        var jumpForce = 0.0f;

        //ForEachの中ではフィールドは呼び出せないらしいので、ここで呼び出す。
        bool _isPushJumpKey = false;
        //処理に時間かかるのか、ダブルジャンプが出来てしまっていたので無理やり修正
        if (isTryJumped)
        {
            isTryJumped = false;
        }
        else
        {
            _isPushJumpKey = jump.ReadValue<float>() > 0;
            isTryJumped = true;
        }

        Entities
        .WithAll<PlayerData>()
        .ForEach((ref PhysicsVelocity _velocity, ref PhysicsMass _mass, ref LocalTransform _transform, ref PlayerData _player) => {
            // ジャンプ確認
            if (_isPushJumpKey && _player.OnGround)
            {
                jumpForce = JUMP_FORCE;
                _player.OnGround = false;
            }
            var _movement = new float3(moveValue.x, jumpForce, moveValue.y);
            var _targetTransform = new RigidTransform(_transform.Rotation, (_movement * 2 * deltaTime) + _transform.Position);
            _velocity = PhysicsVelocity.CalculateVelocityToTarget(_mass, _transform.Position, _transform.Rotation, _targetTransform, tickTime);
            if (_velocity.Linear.y != 0)
            {
                UnityEngine.Debug.LogWarning("linear not 0");
            }

        })
        .ScheduleParallel();

        // 弾発射の確認
        {
            // コンポーネントの取得と更新
            var gun = SystemAPI.GetSingleton<CharacterGunInput>();

            // 存在するか
            if (gun.IsUnityNull())
                return;
            // 反映させる
            gun.Firing = jump.ReadValue<float>() > 0 ? 1 : 0;
            SystemAPI.SetSingleton(gun);
            UnityEngine.Debug.Log("space!" + gun.Firing);

            // jobの発行
            Dependency = new PlayerJob
            {
                Input = gun,

            }.Schedule(Dependency);
        }

        // Jobの発行
        {
            Dependency = new OnGroundJob
            {
                players = GetComponentLookup<PlayerData>(),
                grounds = GetComponentLookup<GroundTag>(),
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
        }

       
    }
}

/// <summary>
/// 
/// </summary>
[BurstCompile]
partial struct PlayerJob : IJobEntity
{
    public CharacterGunInput Input;
    public float shot;

    public void Execute(
        [ChunkIndexInQuery] int index, 
        Entity entity,
        ref LocalTransform xform)
    {

        
    }

}

/// <summary>
/// 地面との当たり判定を行うjob
/// </summary>
public partial struct OnGroundJob : ICollisionEventsJob
{
    public ComponentLookup<PlayerData> players;
    public ComponentLookup<GroundTag> grounds;

    public void Execute(CollisionEvent collisionEvent)
    {
        var _entityAPlayer = players.HasComponent(collisionEvent.EntityA);
        var _entityBPlayer = players.HasComponent(collisionEvent.EntityB);

        var _entityAGround = grounds.HasComponent(collisionEvent.EntityA);
        var _entityBGround = grounds.HasComponent(collisionEvent.EntityB);

        if (_entityBGround | _entityAPlayer | _entityBPlayer | _entityAGround)
            Debug.Log("衝突判定ON");
        
        if (_entityBGround && _entityAPlayer)
        {
            players.GetRefRWOptional(collisionEvent.EntityA).ValueRW.OnGround = true;
        }
        else if (_entityBPlayer && _entityAGround)
        {
            players.GetRefRWOptional(collisionEvent.EntityB).ValueRW.OnGround = true;
        }
    }
}




