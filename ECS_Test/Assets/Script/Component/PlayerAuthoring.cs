using Asset;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private void OnDestroy()
    {
        
    }

    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // �f�t�H���g�ł́A�e�I�[�T�����OGameObject��Entity�ɂȂ�
            // GameObject�i�܂��̓I�[�T�����OComponent�j���^������ƁAGetEntity��Entity���������ĕԂ�
            AddComponent(entity, new PlayerData
            {
                Prefab = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic)
            });

            Debug.Log("�ϊ�����");

        }
    }
}

