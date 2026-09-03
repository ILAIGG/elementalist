using Godot;

public interface IEnemy
{
    HealthSystem Health { get; }
    void ApplyStatusEffect(StatusEffect effect);
    void ApplyKnockback(Vector2 force);
}
