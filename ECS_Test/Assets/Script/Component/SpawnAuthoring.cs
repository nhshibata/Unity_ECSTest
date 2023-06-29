using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public interface ISpawnSettings
{
    float3 Position { get; set; }
    quaternion Rotation { get; set; }

    Entity Prefab { get; set; }
    int Count { get; set; }
    float3 Range { get; set; }

    int RandomSeedOffset { get; set; }
};

[Serializable]
public struct SpawnSettings : IComponentData, ISpawnSettings
{
    public float3 Position { get; set; }
    public quaternion Rotation { get; set; }

    public Entity Prefab { get; set; }
    public int Count { get; set; }
    public float3 Range { get; set; }

    public int RandomSeedOffset { get; set; }
}

public class SpawnAuthoring : MonoBehaviour
{
    [SerializeField] GameObject m_Prefab;

    [SerializeField] int m_nCount;

    [SerializeField] Vector3 m_fRange;
    [SerializeField] int m_nRandomSeedOffset;

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }
    public int NCount { get => m_nCount; set => m_nCount = value; }

    public Vector3 FRange { get => m_fRange; set => m_fRange = value; }
    public int NRandomSeedOffset { get => m_nRandomSeedOffset; set => m_nRandomSeedOffset = value; }

    class Baker : Baker<SpawnAuthoring>
    {
        public override void Bake(SpawnAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // デフォルトでは、各オーサリングGameObjectはEntityになる
            // GameObject（またはオーサリングComponent）が与えられると、GetEntityはEntityを検索して返す
            AddComponent(entity, new SpawnSettings
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Position = authoring.transform.position,
                Rotation = authoring.transform.rotation,
                Count = authoring.NCount,
                Range = authoring.FRange,
                RandomSeedOffset = authoring.NRandomSeedOffset,
            });

        }
    }
}
   
