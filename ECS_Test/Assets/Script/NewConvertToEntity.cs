using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[System.Serializable]
public struct EntityComponent : IComponentData
{
    [SerializeField] Entity prefab;

    public Entity Prefab { get => prefab; set => prefab = value; }
}

public class NewConvertToEntity : MonoBehaviour
{
  
}


class ConvertBaker : Baker<NewConvertToEntity>
{
    public override void Bake(NewConvertToEntity authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new EntityComponent
        {
            // �f�t�H���g�ł́A�e�I�[�T�����OGameObject��Entity�ɕϊ�����܂��B
            // GameObject�i�܂��̓I�[�T�����O�R���|�[�l���g�j���^������ƁAGetEntity�͐��������Entity���������܂��B
            Prefab = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic),

        });
    }
}
