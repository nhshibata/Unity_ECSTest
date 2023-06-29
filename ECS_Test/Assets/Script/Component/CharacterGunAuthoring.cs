using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct CharacterGun : IComponentData
{
    public Entity Bullet;
    public float Strength;
    public float Rate;
    public float Duration;

    public int WasFiring;
    public int IsFiring;
}

public struct CharacterGunInput : IComponentData
{
    public float2 Looking;
    public float Firing;
}

public class CharacterGunAuthoring : MonoBehaviour
{
    public GameObject Bullet;

    public float Strength;
    public float Rate;

    class CharacterGunBaker : Baker<CharacterGunAuthoring>
    {
        public override void Bake(CharacterGunAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            // èeÇÃíËã`
            AddComponent(entity, new CharacterGun()
            {
                Bullet = GetEntity(authoring.Bullet, TransformUsageFlags.Dynamic),
                Strength = authoring.Strength,
                Rate = authoring.Rate,
                WasFiring = 0,
                IsFiring = 0
            });

            // ì¸óÕÇÃí«â¡
            AddComponent(entity, new CharacterGunInput()
            {
                Looking = float2.zero,
                Firing = 0.0f
            });
        }
    }
}