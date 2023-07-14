using Unity.Entities;
using UnityEngine;

partial struct ParticleTag : IComponentData
{

};

public class ParticleAuthoring : MonoBehaviour
{
    class Baker : Baker<ParticleAuthoring>
    {
        public override void Bake(ParticleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity,  new ParticleTag { });
        }
    }
}

