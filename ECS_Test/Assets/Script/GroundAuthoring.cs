using Unity.Entities;
using UnityEngine;

/// <summary>
/// システム側から識別するためのタグ
/// </summary>
[System.Serializable]
public struct GroundTag : IComponentData
{

}

class GroundAuthoring : MonoBehaviour
{

    class GroundBaker : Baker<GroundAuthoring>
    {
        public override void Bake(GroundAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GroundTag
            {

            });
        }
    }
}
