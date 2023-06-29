using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Player�̃V�X�e������
/// </summary>
[BurstCompile]
public partial class PlayerSystem : SystemBase
{
    InputAction move;
    InputAction jump;
    Asset.PlayerInputAction playerInput;
    bool isTryJumped;

    protected override void OnCreate()
    {
        playerInput = new Asset.PlayerInputAction();
        playerInput.Enable();

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
        // ���ʂ���X�V����
        //foreach (var (player, _entity) in SystemAPI.Query<RefRW<PlayerData>>().WithEntityAccess())
        //{
        //    player.ValueRW.IsJump = false;
        //}

        Vector2 _input = move.ReadValue<Vector2>();

        var _deltaTime = UnityEngine.Time.deltaTime;
        var _tickTime = 1f / _deltaTime;
        var _jumpForce = 0.0f;
        //ForEach�̒��ł̓t�B�[���h�͌Ăяo���Ȃ��炵���̂ŁA�����ŌĂяo���B
        bool _isPushJumpKey = false;
        //�����Ɏ��Ԃ�����̂��A�_�u���W�����v���o���Ă��܂��Ă����̂Ŗ������C��
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
            if (_isPushJumpKey && _player.OnGround)
            {
                _jumpForce = 100.0f;
                _player.OnGround = false;
            }
            var _movement = new float3(_input.x, _jumpForce, _input.y);
            var _targetTransform = new RigidTransform(_transform.Rotation, (_movement * 2 * _deltaTime) + _transform.Position);
            _velocity = PhysicsVelocity.CalculateVelocityToTarget(_mass, _transform.Position, _transform.Rotation, _targetTransform, _tickTime);
            if (_velocity.Linear.y != 0)
            {
                UnityEngine.Debug.LogWarning("linear not 0");
            }

        })
        .ScheduleParallel();

        {
            // �R���|�[�l���g�̎擾�ƍX�V
            var gun = SystemAPI.GetSingleton<CharacterGunInput>();
            gun.Firing = jump.ReadValue<float>() > 0 ? 1 : 0;
            SystemAPI.SetSingleton(gun);
            UnityEngine.Debug.Log("space!" + gun.Firing);

            // �W���u�̔��s
            Dependency = new PlayerJob
            {
                Input = gun,

            }.Schedule(Dependency);
        }

        // Job�̔��s
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

public partial struct OnGroundJob : ICollisionEventsJob
{
    public ComponentLookup<PlayerData> players;
    public ComponentLookup<GroundTag> grounds;

    public void Execute(CollisionEvent collisionEvent)
    {
        var _entityAPlayer = players.HasComponent(collisionEvent.EntityA);
        var _entityAGround = grounds.HasComponent(collisionEvent.EntityB);

        var _entityBPlayer = players.HasComponent(collisionEvent.EntityA);
        var _entityBGround = grounds.HasComponent(collisionEvent.EntityB);

        if (_entityBGround | _entityAPlayer | _entityBPlayer | _entityAGround)
            Debug.Log("�Փ˔���ON");
        
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




