using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


class InputAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    float f;
}

class InputBaker : Baker<InputAuthoring>
{
    public override void Bake(InputAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        Debug.Log("�ϊ�");
        AddComponent(entity, new InputComponent
        {
            // �f�t�H���g�ł́A�e�I�[�T�����OGameObject��Entity�ɕϊ�����܂��B
            // GameObject�i�܂��̓I�[�T�����O�R���|�[�l���g�j���^������ƁAGetEntity�͐��������Entity���������܂��B
            Prefab = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic),
            MoveDirection = new float2(0, 0),
        }); 

    }
}
