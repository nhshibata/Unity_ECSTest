using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// �R���|�[�l���g�e�X�g�쐬
/// �v���p�e�B�ɂ���K�v�͂Ȃ��H
/// </summary>
[System.Serializable]
public struct SpawnComponent : IComponentData
{
    [SerializeField] Entity prefab;
    [SerializeField] float3 spawnPosition;
    [SerializeField] float nextSpawnTime;
    [SerializeField] float spawnRate;

    public Entity Prefab { get => prefab; set => prefab = value; }
    public float3 SpawnPosition { get => spawnPosition; set => spawnPosition = value; }
    public float NextSpawnTime { get => nextSpawnTime; set => nextSpawnTime = value; }
    public float SpawnRate { get => spawnRate; set => spawnRate = value; }
}
