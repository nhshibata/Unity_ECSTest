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
            // デフォルトでは、各オーサリングGameObjectはEntityに変換されます。
            // GameObject（またはオーサリングコンポーネント）が与えられると、GetEntityは生成されるEntityを検索します。
            Prefab = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic),

        });
    }
}
