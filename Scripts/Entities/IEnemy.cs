using Godot;

public interface IEnemy
{
    HealthSystem Health { get; }
    Element ElementType { get; }
    void TakeElementalDamage(float amount, Element attackElement, Vector2 position, SceneTree tree, ulong entityId = 0);
    void ApplyStatusEffect(StatusEffect effect);
    void ApplyKnockback(Vector2 force);
}
