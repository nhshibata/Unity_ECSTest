using Unity.Burst;
using Unity.Transforms;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// partial�ɂ��Ă���̂�Unity���Ŏ�����������鏈�������̃N���X�ɓK�p���邽�߂ɕK�{�ł�
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
        // ���ׂĂ�Spawner�R���|�[�l���g���N�G�����܂��B���̃V�X�e���́A
        // �R���|�[�l���g����ǂݎ��Ə������݂��s���K�v�����邽�߁ARefRW���g�p���܂��B
        // �V�X�e�����ǂݎ���p�̃A�N�Z�X�݂̂�K�v�Ƃ���ꍇ�́ARefRO���g�p���܂��B
        foreach (RefRW<SpawnComponent> spawner in SystemAPI.Query<RefRW<SpawnComponent>>())
        {
            ProcessSpawner(ref state, spawner);
        }
    }

    private void ProcessSpawner(ref SystemState state, RefRW<SpawnComponent> spawner)
    {
        // ���̃X�|�[�����Ԃ��o�߂��Ă���ꍇ    
        if (spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime)
        {
            // �X�|�i�[�̎��v���t�@�u���g���āA�V�����G���e�B�e�B�𐶐����܂��B
            Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
            // LocalPosition.FromPosition�́A�w�肳�ꂽ�ʒu�ŏ��������ꂽTransform��Ԃ��܂��B
            state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition));

            // ���̃X�|�[�����Ԃ����Z�b�g���܂��B
            spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
        }
    }
}
