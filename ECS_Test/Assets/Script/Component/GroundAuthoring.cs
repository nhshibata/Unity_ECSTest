using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// ���ʗp��Ground
/// </summary>
public struct GroundTag : IComponentData
{

}

/// <summary>
/// �I�[�T�����O
/// </summary>
public class GroundAuthoring : MonoBehaviour
{
    class Baker : Baker<GroundAuthoring>
    {
        public override void Bake(GroundAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new GroundTag
            {
                
            });

        }
    }
}
