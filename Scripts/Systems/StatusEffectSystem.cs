using System.Collections.Generic;
using System.Linq;

public abstract class StatusEffect
{
    protected StatusEffect(string id, float movementFactor, float duration)
    {
        Id = id;
        MovementFactor = movementFactor;
        RemainingDuration = duration;
    }

    public string Id { get; }
    public float MovementFactor { get; }
    public float RemainingDuration { get; private set; }

    public bool IsExpired => RemainingDuration <= 0f;

    public void Update(double delta)
    {
        RemainingDuration -= (float)delta;
    }
}

public sealed class FrozenEffect : StatusEffect
{
    public FrozenEffect(float movementFactor, float duration)
        : base("frozen", movementFactor, duration)
    {
    }
}

public sealed class StatusEffectSystem
{
    private readonly Dictionary<string, StatusEffect> _effects = new();

    public float MovementFactor
    {
        get
        {
            float movementFactor = 1f;
            foreach (StatusEffect effect in _effects.Values)
                movementFactor = System.MathF.Min(movementFactor, effect.MovementFactor);

            return movementFactor;
        }
    }

    public bool Has<T>() where T : StatusEffect
    {
        return _effects.Values.Any(effect => effect is T);
    }

    public void Apply(StatusEffect effect)
    {
        if (effect.IsExpired)
            return;

        if (_effects.TryGetValue(effect.Id, out StatusEffect currentEffect)
            && effect.MovementFactor >= currentEffect.MovementFactor)
            return;

        _effects[effect.Id] = effect;
    }

    public void Update(double delta)
    {
        foreach (StatusEffect effect in _effects.Values)
            effect.Update(delta);

        List<string> expiredEffects = new();
        foreach (KeyValuePair<string, StatusEffect> entry in _effects)
        {
            if (entry.Value.IsExpired)
                expiredEffects.Add(entry.Key);
        }

        foreach (string effectId in expiredEffects)
            _effects.Remove(effectId);
    }
}