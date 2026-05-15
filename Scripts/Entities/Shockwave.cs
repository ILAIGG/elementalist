using Godot;

public partial class Shockwave : Area2D
{
    [Export] public float Damage = 5f;
    [Export] public float Duration = 2f;
    [Export] public float Radius = 120f;

    private float _timer = 0f;
    private float _damageTick = 0f;
    private const float DamageInterval = 0.5f;

    public override void _PhysicsProcess(double delta)
    {
        _timer += (float)delta;
        _damageTick += (float)delta;

        //Hace daño cada 0.5 segundos a los enemigos dentro del área
        if (_damageTick >= DamageInterval)
        {
            _damageTick = 0f;
            ApplyDamage();
        }

        QueueRedraw();

        if (_timer >= Duration)
            QueueFree();
    }

    public override void _Draw()
    {
        float alpha = 1f - (_timer / Duration);
        Color color = new Color(0.7f, 0.3f, 1f, Mathf.Max(0, alpha) * 0.25f);
        DrawCircle(Vector2.Zero, Radius, color);
        DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 64, new Color(0.7f, 0.3f, 1f, Mathf.Max(0, alpha)), 1.5f);
    }

    private void ApplyDamage()
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is IEnemy enemy)
                enemy.Health.TakeDamage(Damage, body.GlobalPosition, GetTree(), body.GetInstanceId());
        }
    }
}