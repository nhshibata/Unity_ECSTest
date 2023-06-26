using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PlayerData : IComponentData
{
    Entity entity;
    bool isJump;
    bool onGround;

    public Entity Prefab { get => entity; set => entity = value; }
    public bool IsJump { get => isJump; set => isJump = value; }
    public bool OnGround { get => onGround; set => onGround = value; }
}

public class PlayerAuthoring : MonoBehaviour
{


    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // デフォルトでは、各オーサリングGameObjectはEntityになる
            // GameObject（またはオーサリングComponent）が与えられると、GetEntityはEntityを検索して返す
            AddComponent(entity, new PlayerData
            {
                Prefab = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic)
            });

        }
    }
}
