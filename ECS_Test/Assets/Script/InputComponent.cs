using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct InputComponent : IComponentData
{
    Entity prefab;
    float2 moveDirection;

    public float2 MoveDirection { get => moveDirection; set => moveDirection = value; }
    public Entity Prefab { get => prefab; set => prefab = value; }
}
