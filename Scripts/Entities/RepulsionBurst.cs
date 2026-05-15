using Godot;

public partial class RepulsionBurst : Area2D
{
    [Export] public float Damage = 5f;
    [Export] public float Duration = 0.3f;
    [Export] public float Force = 200f;
    [Export] public float Radius = 120f;

    private float _timer = 0f;
    private bool _effectApplied = false;

    public bool HasShockwave { get; set; } = false;
    [Export] public PackedScene ShockwaveScene { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        _timer += (float) delta;

        if (!_effectApplied)
        {
            _effectApplied = true;
            ApplyEffect();
        }

        QueueRedraw();

        if (_timer >= Duration)
            QueueFree();
    }

    public override void _Draw()
    {
        float alpha = 1f - (_timer / Duration);
        Color fillColor = new(0.5f, 0f, 1f, Mathf.Max(0, alpha) * 0.3f);
        Color borderColor = new(0.7f, 0.3f, 1f, Mathf.Max(0, alpha));

        DrawCircle(Vector2.Zero, Radius, fillColor);
        DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 64, borderColor, 2f);
    }

    private void ApplyEffect()
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is Enemy enemy)
            {
                //Daño mínimo
                enemy.Health.TakeDamage(Damage, body.GlobalPosition, body.GetTree(), body.GetInstanceId());

                //Fuerza de empuje hacia afuera
                Vector2 pushDirection = (body.GlobalPosition - GlobalPosition).Normalized();
                enemy.ApplyKnockback(pushDirection * Force);
            }
        }

        // Si tiene shockwave, deja una onda persistente
        if (HasShockwave && ShockwaveScene != null)
        {
            Shockwave shockwave = ShockwaveScene.Instantiate<Shockwave>();
            shockwave.Radius = Radius;
            GetParent().AddChild(shockwave);
            shockwave.GlobalPosition = GlobalPosition;
        }
    }
}