//using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

/// <summary>
/// OnCollision用
/// </summary>
public partial struct CollisionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }

    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        // jobスケジュールの発行
        // ここで呼び出すことで実行される？
        state.Dependency = new DestroyCubeJob
        {
            _cubes = state.GetComponentLookup<CubeData>(),
            _players = state.GetComponentLookup<PlayerData>(),
            _ecb = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>().CreateCommandBuffer()
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    /// <summary>
    /// ICollisionEventsJobを使用
    /// ITriggerEventsJobも存在
    /// </summary>
    [BurstCompile]
    public partial struct DestroyCubeJob : ICollisionEventsJob
    {
        public ComponentLookup<CubeData> _cubes;
        public ComponentLookup<PlayerData> _players;
        public EntityCommandBuffer _ecb;

        public void Execute(CollisionEvent _collisionEvent)
        {
            var _entityA = _collisionEvent.EntityA;
            var _entityB = _collisionEvent.EntityB;

            var _isEntityACube = _cubes.HasComponent(_entityA);
            var _isEntityBCube = _cubes.HasComponent(_entityB);

            var _isEntityAPlayer = _players.HasComponent(_entityA);
            var _isEntityBPlayer = _players.HasComponent(_entityB);

            Debug.Log("bb");
            if (_isEntityACube | _isEntityBPlayer | _isEntityAPlayer | _isEntityBCube)
                Debug.Log("aaa");

            if (_isEntityACube && _isEntityBPlayer)
            {
                _ecb.DestroyEntity(_entityA);
            }
            else if (_isEntityAPlayer && _isEntityBCube)
            {
                _ecb.DestroyEntity(_entityB);
            }
        }
    }

}
