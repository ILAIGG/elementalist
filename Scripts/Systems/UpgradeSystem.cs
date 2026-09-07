using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

//Define los tipos de upgrades que existen
public enum UpgradeType
{
    Spell, //Mejora un hechizo
    Ability, //Mejora una habilidad
    Dash, //Mejora el dash
    Stat //Mejora un stat genérico (Vida, Velocidad de movimiento, etc)
}

//Un upgrade individual
public class Upgrade
{
    public string Id; //Identificador único, por ejemplo "fireball_damage"
    public string Name; //Nombre visible, por ejemplo "Furia Arcana"
    public UpgradeType Type;
    public bool IsInfinite; //Se puede tomar más de una vez?
    public int TimesApplied; //Cuantas veces lo tomó el jugador
    public int MaxAcquisitions; //Límite de cuantas veces puede agarrarse (0 = sin límite)

    //Función que modifica al jugador cuando se elige esta mejora
    public Action<Player, int> Apply;

    //En vez de un string fijo, es una función que calcula la descripción según cuantas veces fue aplicado
    public Func<int, string> GetDescription; //Descripción visible
    public Func<int, string> GetDescriptionKey;
    public Func<int, object[]> GetDescriptionArguments;

    //Condición opcional, si es null el upgrade siempre estará disponible, si no es null, solo aparecerá si la condición devuelve true.
    public Func<Player, bool> Condition = null;

    //Ids de upgrades que se bloquean cuando este se toma
    public string[] Excludes = System.Array.Empty<string>();

    public string NameKey => $"upgrade.{Id}.name";
    public string DescriptionKey => $"upgrade.{Id}.description";

    public string GetLocalizedDescriptionKey(int times)
    {
        return GetDescriptionKey?.Invoke(times) ?? DescriptionKey;
    }

    public object[] GetLocalizedDescriptionArguments(int times)
    {
        return GetDescriptionArguments?.Invoke(times) ?? System.Array.Empty<object>();
    }
}

public class UpgradeSystem
{
    //Todos los upgrades disponibles en el juego
    private List<Upgrade> _allUpgrades = new();

    //Los upgrades que el jugador ya tiene
    private List<Upgrade> _acquiredUpgrades = new();

    //Ids de upgrades bloqueados por exclusión
    private HashSet<string> _excludedIds = new();

    private Player _player;
    private PlayerStats _stats;
    private Random _random = new();

    public UpgradeSystem(Player player, PlayerStats stats)
    {
        _player = player;
        _stats = stats;
        RegisterUpgrades();
    }

    //Genera una lista de 3 upgrades para mostrar al jugador
    public List<Upgrade> GetUpgradeChoices()
    {
        List<Upgrade> available = GetAvailableUpgrades();
        List<Upgrade> choices = new();

        //Elegimos 3 upgrades al azar sin repetir
        while (choices.Count < 3 && available.Count > 0)
        {
            int index = _random.Next(available.Count);
            choices.Add(available[index]);
            available.RemoveAt(index);
        }

        return choices;
    }

    private List<Upgrade> GetAvailableUpgrades()
    {
        List<Upgrade> available = new();

        foreach (var upgrade in _allUpgrades)
        {
            //Los upgrades únicos solo aparecen si no fueron tomados
            if (!upgrade.IsInfinite && upgrade.TimesApplied > 0)
                continue;

            //Si es infinito pero tiene un límite máximo de veces (mayor a 0), verificamos no excederlo
            if (upgrade.IsInfinite && upgrade.MaxAcquisitions > 0 && upgrade.TimesApplied >= upgrade.MaxAcquisitions)
                continue;

            //Si tiene condición y no se cumple, no aparece
            if (upgrade.Condition != null && !upgrade.Condition(_player))
                continue;

            //Si está en la lista de excluidos, no aparece
            if (_excludedIds.Contains(upgrade.Id))
                continue;

            available.Add(upgrade);
        }

        return available;
    }

    //Aplica un upgrade al jugador
    public void ApplyUpgrade(Upgrade upgrade)
    {
        upgrade.TimesApplied++;
        upgrade.Apply(_player, upgrade.TimesApplied);

        if (!_acquiredUpgrades.Contains(upgrade))
            _acquiredUpgrades.Add(upgrade);

        //Bloquea todos los upgrades que este excluye
        foreach (var excludedId in upgrade.Excludes)
            _excludedIds.Add(excludedId);
    }

    //Acá se registran todas las upgrades del juego
    private void RegisterUpgrades()
    {
        //-------- STATS BASE --------
        //Upgrades genéricos de stats (siempre disponibles)
        _allUpgrades.Add(new Upgrade
        {
            Id = "stat_health",
            Name = "Arcane Vitality",
            GetDescription = (times) => "+20 Maximum Health.", //Descripción fija
            Type = UpgradeType.Stat,
            IsInfinite = true,
            Apply = (player, times) =>
            {
                _stats.MaxHealth += 20f;
                player.Health.IncreaseMaxHealth(20f);
            }
        });

        _allUpgrades.Add(new Upgrade
        {
            Id = "stat_speed",
            Name = "Ethereal Stride",
            GetDescription = (times) => "+15 Movement Speed.",
            Type = UpgradeType.Stat,
            IsInfinite = true,
            Apply = (player, times) =>
            {
                _stats.Speed += 15f;
            }
        });

        _allUpgrades.Add(new Upgrade
        {
            Id = "stat_damage",
            Name = "Arcane Might",
            GetDescription = (times) => "+6 Damage to ALL spells and abilities.",
            Type = UpgradeType.Stat,
            IsInfinite = true,
            Apply = (player, times) =>
            {
                _stats.BonusDamage += 6f;
            }
        });

        //Upgrade escalable de regeneración de vida
        _allUpgrades.Add(new Upgrade
        {
            Id = "stat_regen",
            Name = "Arcane Regeneration",
            GetDescription = (times) => $"+{times} HP regenerated every 3 seconds.",
            GetDescriptionArguments = (times) => new object[] { times },
            Type = UpgradeType.Stat,
            IsInfinite = true,
            Apply = (player, times) =>
            {
                _stats.HealthRegen += 1;
            }
        });

        //-------- DASH --------
        //Invencibilidad durante el dash
        _allUpgrades.Add(new Upgrade
        {
            Id = "dash_invincible",
            Name = "Shadow Dash",
            GetDescription = (times) => "The mage is invincible and passes through enemies during the dash.",
            Type = UpgradeType.Dash,
            IsInfinite = false,
            Apply = (player, times) =>
            {
                _stats.DashIsInvincible = true;
            }
        });

        //Reduce el cooldown del dash
        _allUpgrades.Add(new Upgrade
        {
            Id = "dash_cooldown",
            Name = "Arcane Impulse",
            GetDescription = (times) => $"-0.2s Dash Cooldown (Current: {_player.DashCooldown:F2}s)",
            GetDescriptionArguments = (times) => new object[] { _player.DashCooldown },
            Type = UpgradeType.Dash,
            IsInfinite = true,
            Apply = (player, times) =>
            {
                _player.DashCooldown = Mathf.Max(0.5f, player.DashCooldown - 0.2f);
            },
            Condition = (player) => player.DashCooldown > 0.5f,
        });

        //-------- FIREBALL --------
        //Upgrade único de hechizo (cambia su comportamiento)
        //Dispara en ráfaga secuencial en línea recta
        _allUpgrades.Add(new Upgrade
        {
            Id = "fireball_burst",
            Name = "Burst Fire",
            GetDescription = (times) => _stats.FireballBurstCount == 0
                ? $"Fires 2 Fireball projectiles in burst. Fireball damage is reduced by 15%. (Current: {_stats.FireballBurstCount})"
                : $"+1 Fireball projectile fired in burst. (Current: {_stats.FireballBurstCount})",
            GetDescriptionKey = (times) => _stats.FireballBurstCount == 0
                ? "upgrade.fireball_burst.first_description"
                : "upgrade.fireball_burst.description",
            GetDescriptionArguments = (times) => new object[] { _stats.FireballBurstCount },
            Type = UpgradeType.Spell,
            IsInfinite = true,
            MaxAcquisitions = 9, //Primera vez +2, luego 8 veces +1 = 10 proyectiles máximo
            Condition = (player) => !_excludedIds.Contains("fireball_burst"),
            Excludes = new[] { "fireball_multishot" },
            Apply = (player, times) =>
            {
                //Primera vez da 2 proyectiles, las siguientes +1
                _stats.FireballBurstCount += (times == 1) ? 2 : 1;
            }
        });

        _allUpgrades.Add(new Upgrade
        {
            Id = "fireball_piercing",
            Name = "Piercing Fireball",
            GetDescription = (times) => "Fireball now pierces through enemies.",
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Condition = (player) => !_excludedIds.Contains("fireball_piercing"),
            Excludes = new[] { "fireball_explosive" },
            Apply = (player, times) =>
            {
                _stats.FireballPiercing = true;
            }
        });

        _allUpgrades.Add(new Upgrade
        {
            Id = "fireball_multishot",
            Name = "Multishot",
            GetDescription = (times) => $"+2 Fireball projectile (Current: {_stats.FireballCount})",
            GetDescriptionArguments = (times) => new object[] { _stats.FireballCount },
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Condition = (player) => _stats.FireballCount <= 10 && !_excludedIds.Contains("fireball_multishot"),
            Excludes = new[] { "fireball_burst" },
            Apply = (player, times) =>
            {
                _stats.FireballCount += 2;
            },
        });

        //Bola de fuego explosiva
        _allUpgrades.Add(new Upgrade
        {
            Id = "fireball_explosive",
            Name = "Explosive Fireball",
            GetDescription = (times) => "Fireball now explodes on impact, dealing Area-of-Effect(AoE) damage.",
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Condition = (player) => !_excludedIds.Contains("fireball_explosive"),
            Excludes = new[] { "fireball_piercing" },
            Apply = (player, times) =>
            {
                _stats.FireballExplosive = true;
            }
        });

        //Upgrade stackeable de hechizo (escala con veces aplicado)
        _allUpgrades.Add(new Upgrade
        {
            Id = "fireball_damage",
            Name = "Igneous Fury",
            GetDescription = (times) => $"+{2f * times} Fireball Damage. This bonus scales up further every time you choose this upgrade card.", //Descripción dinámica. Muestra exactamente cuánto daño va a sumar esta vez
            GetDescriptionArguments = (times) => new object[] { 2f * times },
            Type = UpgradeType.Spell,
            IsInfinite = true,
            Apply = (player, times) =>
            {
                //Primer vez +5, segunda +10, tercera +15...
                _stats.BonusFireballDamage += 2f * times;
            }
        });

        //-------- FROST RAY --------
        //Upgrade único que desbloquea un rayo de hielo
        _allUpgrades.Add(new Upgrade
        {
            Id = "unlock_frost_ray",
            Name = "Frost Ray",
            GetDescription = (times) => "A beam that pierces and damages enemies it hits.",
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Apply = (player, times) =>
            {
                _stats.HasFrostRay = true;
            }
        });

        // Permafrost desactivado temporalmente junto con FrozenEffect.
        // _allUpgrades.Add(new Upgrade
        // {
        //     Id = "frost_ray_permafrost",
        //     Name = "Permafrost",
        //     GetDescription = (times) => "Frost Ray now COMPLETELY freezes enemies hit. Freeze duration is halved. The bosses can't be completely frozen.",
        //     Type = UpgradeType.Spell,
        //     IsInfinite = false,
        //     Condition = (player) => _stats.HasFrostRay,
        //     Apply = (player, times) =>
        //     {
        //         _stats.FrostRaySlowFactor = 0f;
        //         _stats.FrostRaySlowDuration /= 2f;
        //     }
        // });

        _allUpgrades.Add(new Upgrade
        {
            Id = "frost_ray_wide",
            Name = "Wide Beam",
            GetDescription = (times) => "Frost Ray becomes wider, hitting more enemies at once.",
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Condition = (player) => _stats.HasFrostRay && !_excludedIds.Contains("frost_ray_wide"),
            Excludes = new[] { "frost_ray_chain" },
            Apply = (player, times) =>
            {
                _stats.FrostRayWidth = 40f;
            }
        });

        _allUpgrades.Add(new Upgrade
        {
            Id = "frost_ray_chain",
            Name = "Chaining Frost",
            GetDescription = (times) => "Frost Ray bounces to the nearest enemy after hitting one.",
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Condition = (player) => _stats.HasFrostRay && !_excludedIds.Contains("frost_ray_chain"),
            Excludes = new[] { "frost_ray_wide" },
            Apply = (player, times) =>
            {
                _stats.FrostRayChain = true;
            }
        });

        // Lingering Chill desactivado temporalmente junto con FrozenEffect.
        // _allUpgrades.Add(new Upgrade
        // {
        //     Id = "frost_ray_slow_duration",
        //     Name = "Lingering Chill",
        //     GetDescription = (times) => (_stats.FrostRaySlowFactor != 0) ? $"+1s Frost Ray slow duration." : $"+0.5s Frost Ray freeze duration.",
        //     GetDescriptionKey = (times) => _stats.FrostRaySlowFactor != 0
        //         ? "upgrade.frost_ray_slow_duration.description"
        //         : "upgrade.frost_ray_slow_duration.freeze_description",
        //     Type = UpgradeType.Spell,
        //     IsInfinite = true,
        //     Condition = (player) => _stats.HasFrostRay,
        //     Apply = (player, times) =>
        //     {
        //         _stats.FrostRaySlowDuration += (_stats.FrostRaySlowFactor != 0) ? 1f : 0.5f;
        //     }
        // });

        //-------- REPULSION BURST --------
        //Desbloquea el RepulsionBurst
        _allUpgrades.Add(new Upgrade
        {
            Id = "unlock_repulsion_burst",
            Name = "Repulsion Burst",
            GetDescription = (times) => $"Periodically releases a burst that pushes nearby enemies away. Triggers every {_stats.RepulsionBurstFireRate:F1} seconds.",
            GetDescriptionArguments = (times) => new object[] { _stats.RepulsionBurstFireRate },
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Apply = (player, times) =>
            {
                _stats.HasRepulsionBurst = true;
            }
        });

        _allUpgrades.Add(new Upgrade
        {
            Id = "repulsion_burst_damage",
            Name = "Arcane Repulsion",
            GetDescription = (times) => $"+2 Repulsion Burst damage (Current: {_stats.BonusRepulsionBurstDamage:F2}s).",
            GetDescriptionArguments = (times) => new object[] { _stats.BonusRepulsionBurstDamage },
            Type = UpgradeType.Spell,
            IsInfinite = true,
            Condition = (player) => _stats.HasRepulsionBurst,
            Apply = (player, times) =>
            {
                _stats.BonusRepulsionBurstDamage += 3f;
            }
        });

        _allUpgrades.Add(new Upgrade
        {
            Id = "repulsion_burst_shockwave",
            Name = "Shockwave",
            GetDescription = (times) => "Repulsion Burst leaves a shockwave that damages enemies for 2 seconds.",
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Condition = (player) => _stats.HasRepulsionBurst && !_excludedIds.Contains("repulsion_burst_shockwave"),
            Excludes = new[] { "repulsion_burst_extended" },
            Apply = (player, times) =>
            {
                _stats.RepulsionBurstShockwave = true;
            }
        });

        _allUpgrades.Add(new Upgrade
        {
            Id = "repulsion_burst_extended",
            Name = "Extended Burst",
            GetDescription = (times) => "+60 Repulsion Burst range and +100 push force.",
            Type = UpgradeType.Spell,
            IsInfinite = false,
            Condition = (player) => _stats.HasRepulsionBurst && !_excludedIds.Contains("repulsion_burst_extended"),
            Excludes = new[] { "repulsion_burst_shockwave" },
            Apply = (player, times) =>
            {
                _stats.RepulsionBurstRange += 60f;
                _stats.RepulsionBurstForce += 100f;
            }
        });

        //-------- NOVA DE FUEGO --------
        //La Nova lanza 3 bolas de fuego al explotar
        _allUpgrades.Add(new Upgrade
        {
            Id = "nova_chain",
            Name = "Nova Chain",
            GetDescription = (times) => "Fire Nova now launches 3 Fireballs upon detonating.",
            Type = UpgradeType.Ability,
            IsInfinite = false,
            Apply = (player, times) =>
            {
                _stats.FireNovaChain = true;
            }
        });

        //-0.5 segundos al cooldown de la Nova
        _allUpgrades.Add(new Upgrade
        {
            Id = "nova_cooldown",
            Name = "Nova Frenzy",
            GetDescription = (times) => $"-0.5s Fire Nova Cooldown (Current: {_stats.FireNovaCooldown:F2}s)",
            GetDescriptionArguments = (times) => new object[] { _stats.FireNovaCooldown },
            Type = UpgradeType.Ability,
            IsInfinite = true,
            Apply = (player, times) =>
            {
                _stats.FireNovaCooldown = Mathf.Max(1f, _stats.FireNovaCooldown - 0.5f);
            },
            Condition = (player) => _stats.FireNovaCooldown > 1
        });
    }
}