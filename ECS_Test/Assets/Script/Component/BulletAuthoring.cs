using System;
using Unity.Entities;
using UnityEngine;



/// <summary>
/// 弾のデータ
/// </summary>
[Serializable]
partial struct BulletData : IComponentData
{
    float speed;
    float limitTime;

    public float Speed { get => speed; set => speed = value; }
    public float LimitTime { get => limitTime; set => limitTime = value; }
}

/// <summary>
/// オーサリング処理
/// </summary>
public class BulletAuthoring : MonoBehaviour
{
    [SerializeField]float BulletSpeed = 1;
    [SerializeField]float BulletLimitTime = 1;

    class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new BulletData {
                Speed = authoring.BulletSpeed,
                LimitTime = authoring.BulletLimitTime
            });
        }
    }
}

