using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Ž¯•Ê—p‚ÌGround
/// </summary>

public struct GroundTag : IComponentData
{

}

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
