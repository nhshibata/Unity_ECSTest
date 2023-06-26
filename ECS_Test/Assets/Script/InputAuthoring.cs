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

        Debug.Log("変換");
        AddComponent(entity, new InputComponent
        {
            // デフォルトでは、各オーサリングGameObjectはEntityに変換されます。
            // GameObject（またはオーサリングコンポーネント）が与えられると、GetEntityは生成されるEntityを検索します。
            Prefab = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic),
            MoveDirection = new float2(0, 0),
        }); 

    }
}
