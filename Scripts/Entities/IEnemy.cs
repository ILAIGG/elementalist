using Godot;

public interface IEnemy
{
    HealthSystem Health { get; }
    void ApplySlow(float factor, float duration);
    void ApplyKnockback(Vector2 force);
}
