using System;

public class PlayerStats
{
    //Stats base
    public float Speed = 180f;
    public float MaxHealth = 100f;
    public float HealthRegen = 0f;
    public float BonusDamage = 0f;

    //Fireball
    public float BonusFireballDamage = 0;
    public float FireballRange = 550f;
    public int FireballCount = 1;
    public int FireballBurstCount = 0; //Cantidad de proyectiles en ráfaga (0 = sin ráfaga)
    public bool FireballExplosive = false;
    public bool FireballPiercing = false;

    //Frost Ray
    public bool HasFrostRay = false;
    public float BonusFrostRayDamage = 0;
    public float FrostRaySlowFactor = 0.4f;
    public float FrostRaySlowDuration = 2f;
    public float FrostRayRange = 200f;
    public float FrostRayWidth = 8f;  //ancho base del rayo
    public bool FrostRayChain = false;

    //Fire Nova
    public float BonusFireNovaDamage = 0f;
    public float FireNovaCooldown = 5f;
    public bool FireNovaChain = false;

    //Meteor Shower
    public float BonusMeteorShowerDamage = 0f;
    public float MeteorShowerCooldown = 8f;
    public int MeteorShowerCount = 5;
    public float MeteorShowerSpread = 100f;

    //Repulsion Burst
    public bool HasRepulsionBurst = false;
    public float RepulsionBurstRange = 120f;
    public float RepulsionBurstForce = 200f;
    public float RepulsionBurstFireRate = 4f;
    public float BonusRepulsionBurstDamage = 0f;
    public bool RepulsionBurstShockwave = false;

    //Dash
    public bool DashIsInvincible = false;
}