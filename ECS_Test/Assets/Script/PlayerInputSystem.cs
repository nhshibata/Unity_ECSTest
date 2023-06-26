using System.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using UnityEngine;

[BurstCompile]
public partial struct PlayerInputSystem : ISystem
{
    public void OnCreate(ref SystemState system)
    {
    }
    public void OnDestroy(ref SystemState system)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState system)
    {        
        float deltaTime = Time.deltaTime;
        float horizontalInput = UnityEngine.Input.GetAxis("Horizontal");
        float verticalInput = UnityEngine.Input.GetAxis("Vertical");
        //Debug.Log("â°"+ horizontalInput + "èc" + verticalInput);
        foreach (var (transform, input) in
                    SystemAPI.Query<RefRW<LocalTransform>, RefRW<InputComponent>>())
        {
            // ValueRW and ValueRO both return a ref to the actual component value.
            // The difference is that ValueRW does a safety check for read-write access while
            // ValueRO does a safety check for read-only access.
            //transform.ValueRW = transform.ValueRO.RotateY(
            //    speed.ValueRO.RadiansPerSecond * deltaTime);

            Debug.Log("â°èc");
            input.ValueRW.MoveDirection = new float2(horizontalInput, verticalInput);
            
            //Debug.Log("ç¿ïW" + transform.ValueRW.Position);
            transform.ValueRW.Position += 
                new float3(input.ValueRO.MoveDirection.x, 0f, input.ValueRO.MoveDirection.y) * deltaTime;            
        }

        
    }
}

[BurstCompile]
public partial struct ProcessInputJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;

    void Execute([ChunkIndexInQuery] int chunkIndex,
               Entity entity,
               ref LocalTransform xForm,
                ref InputComponent input)
    {
        float3 newPos = xForm.Position;
        newPos.x += input.MoveDirection.x;
        newPos.y += input.MoveDirection.y;
        Ecb.SetComponent(chunkIndex, entity, LocalTransform.FromPosition(newPos));

        float horizontalInput = UnityEngine.Input.GetAxis("Horizontal");
        float verticalInput = UnityEngine.Input.GetAxis("Vertical");
        input.MoveDirection = new float2(horizontalInput, verticalInput);
        
    }

}