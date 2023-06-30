using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// ƒ^ƒO‚Æ‚µ‚Ä‚àˆµ‚¤?
/// </summary>
[Serializable]
partial struct EnemyData : IComponentData
{
    float speed;

    public float Speed { get => speed; set => speed = value; }
}

public class EnemyAuthoring : MonoBehaviour
{
    float speed;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new EnemyData
            {
                Speed = authoring.speed,
            });

        }
    }
}

