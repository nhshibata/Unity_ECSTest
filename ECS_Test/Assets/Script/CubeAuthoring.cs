using Unity.Entities;
using UnityEngine;

[System.Serializable]
public struct CubeData : IComponentData
{

}

class CubeAuthoring : MonoBehaviour
{
    public float a;
    
    class CubeBaker : Baker<CubeAuthoring>
    {
        public override void Bake(CubeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CubeData
            {

            });
        }
    }


}

