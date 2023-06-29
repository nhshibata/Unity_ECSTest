using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[Serializable]
partial struct ItemData : IComponentData
{
    int point;

    public int Point { get => point; set => point = value; }
}

public class ItemAuthoring : MonoBehaviour
{
    public int Point = 1;

    class Baker : Baker<ItemAuthoring>
    {
        public override void Bake(ItemAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new ItemData {
                Point  = authoring.Point
            });



        }
    }
}
