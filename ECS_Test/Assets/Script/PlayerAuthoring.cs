using Asset;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct PlayerData : IComponentData
{
    [SerializeField]Entity prefab;
    bool onGround;

    public Entity Prefab { get => prefab; set => prefab = value; }
    public bool OnGround { get => onGround; set => onGround = value; }
}

class PlayerAuthoring : MonoBehaviour
{

    class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PlayerData
            {
                // デフォルトでは、各オーサリングGameObjectはEntityに変換されます。
                // GameObject（またはオーサリングコンポーネント）が与えられると、GetEntityは生成されるEntityを検索します。
                Prefab = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic),
            });
        }

    }
}

public partial class PlayerMoveSystem : SystemBase
{
    PlayerInputActions playerInputActionScript;
    InputAction move;
    InputAction jump;
    bool isTryJumped = false;

    protected override void OnCreate()
    {
        playerInputActionScript = new PlayerInputActions();
        playerInputActionScript.Enable();

        move = playerInputActionScript.Player.Move;
        jump = playerInputActionScript.Player.Jump;
    }

    protected override void OnDestroy()
    {
        playerInputActionScript.Disable();
        playerInputActionScript.Dispose();
    }

    protected override void OnUpdate()
    {
        Vector2 _input = move.ReadValue<Vector2>();

        var _deltaTime = UnityEngine.Time.deltaTime;
        var _tickTime = 1f / _deltaTime;
        var _jumpForce = 0.0f;
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
                //Debug.LogWarning(_velocity.Linear);
                Debug.LogWarning("Linear");
            }
        })
        .ScheduleParallel();


        Dependency = new OnGroundJob
        {
            _players = GetComponentLookup<PlayerData>(),
            _grounds = GetComponentLookup<GroundTag>(),
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

    }
}

[BurstCompile]
public partial struct OnGroundJob : ICollisionEventsJob
{
    public ComponentLookup<PlayerData> _players;
    public ComponentLookup<GroundTag> _grounds;

    public void Execute(CollisionEvent collisionEvent)
    {
        var _entityAPlayer = _players.HasComponent(collisionEvent.EntityA);
        var _entityAGround = _grounds.HasComponent(collisionEvent.EntityB);

        var _entityBPlayer = _players.HasComponent(collisionEvent.EntityA);
        var _entityBGround = _grounds.HasComponent(collisionEvent.EntityB);

        Debug.Log("地面との確認");
        if (_entityBGround | _entityAPlayer | _entityBPlayer | _entityAGround)
            Debug.Log("接触");

        if (_entityBGround && _entityAPlayer)
        {
            _players.GetRefRWOptional(collisionEvent.EntityA).ValueRW.OnGround = true;
        }
        else if (_entityBPlayer && _entityAGround)
        {
            _players.GetRefRWOptional(collisionEvent.EntityB).ValueRW.OnGround = true;   
        }
    }
}