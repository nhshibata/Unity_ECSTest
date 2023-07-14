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

    private int nWasFiring;
    private int nIsFiring;

    public int WasFiring { get => nWasFiring; set => nWasFiring = value; }

    public int IsFiring { get => nIsFiring; set => nIsFiring = value; }
}

public struct CharacterGunInput : IComponentData
{
    private float2 vLooking;
    private float fFiring;

    public float2 Looking { get => vLooking; set => vLooking = value; }
    public float Firing { get => fFiring; set => fFiring = value; }
}

public class CharacterGunAuthoring : MonoBehaviour
{
    public GameObject pPrefabBullet;

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
                Bullet = GetEntity(authoring.pPrefabBullet, TransformUsageFlags.Dynamic),
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