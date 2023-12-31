﻿using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

[BurstCompile]
public partial struct GunInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // 指定したコンポーネントがなければ実行しない
        state.RequireForUpdate<CharacterGunInput>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
           .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        // 一つしかないため、Singletonで取得
        //var gun = SystemAPI.GetSingleton<CharacterGunInput>();

        // データの取得と発行
        foreach (var (gund, transform) in
                    SystemAPI.Query<CharacterGunInput, LocalTransform>())
        {
            // ジョブの発行
            new GunInputJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Input = gund,
                ECB = ecb
            }.ScheduleParallel();
            
            // 取得完了したため抜ける
            break;
        }
        
    }

}

/// <summary>
/// 入力時発射job
/// </summary>
[BurstCompile]
partial struct GunInputJob : IJobEntity
{
    public float DeltaTime;
    public CharacterGunInput Input;
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(
        [ChunkIndexInQuery] int chunkIndexInQuery, 
        ref LocalTransform gunLocalTransform,           
        ref CharacterGun gun, 
        in LocalToWorld gunTransform)
    {
        // Handle input
        {
            float a = -Input.Looking.y;

            gunLocalTransform.Rotation =
                math.mul(gunLocalTransform.Rotation, quaternion.Euler(math.radians(a), 0, 0));

            gun.IsFiring = Input.Firing > 0f ? 1 : 0;
        }

        UnityEngine.Debug.Log("ジョブの発行はされている");

        // 入力確認
        if (gun.IsFiring == 0)
        {
            gun.Duration = 0;
            gun.WasFiring = 0;
            return;
        }

        // 発射できる入力時間か
        gun.Duration += DeltaTime;
        if ((gun.Duration > gun.Rate) || (gun.WasFiring == 0))
        {
            if (gun.Bullet != null)
            {
                var e = ECB.Instantiate(chunkIndexInQuery, gun.Bullet);

                // 生成したEntityにアタッチする
                LocalTransform localTransform = LocalTransform.FromPositionRotationScale(
                    gunTransform.Position + gunTransform.Forward,
                    gunLocalTransform.Rotation,
                    gunLocalTransform.Scale);

                PhysicsVelocity velocity = new PhysicsVelocity
                {
                    Linear = gunTransform.Forward * gun.Strength,
                    Angular = float3.zero
                };

                UnityEngine.Debug.Log("job 発射");

                // コンポーネントの設定
                ECB.AddComponent(chunkIndexInQuery, e, localTransform);
                ECB.AddComponent(chunkIndexInQuery, e, velocity);
                //ECB.SetComponent(chunkIndexInQuery, e, localTransform);
                //ECB.SetComponent(chunkIndexInQuery, e, velocity);

            }

            gun.Duration = 0;
        }

        gun.WasFiring = 1;
    }
}