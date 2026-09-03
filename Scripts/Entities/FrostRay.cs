using System.Reflection.Metadata;
using Godot;

public partial class FrostRay : Area2D
{
    [Export] public float Damage = 8f;
    [Export] public float Duration = 0.15f; //Segundos que dura el rayo
    [Export] public float SlowFactor = 0.4f; //Multiplica la velocidad del enemigo
    [Export] public float SlowDuration = 2f; //Segundos que dura el slow

    public bool IsChaining { get; set; } = false;
    private bool _hasChained = false;

    private float _timer = 0f;
    private bool _damageApplied = false;
    private int _frameCount = 0;

    public override void _PhysicsProcess(double delta)
    {
        _timer += (float)delta;

        if (!_damageApplied)
        {
            _frameCount++;
            if(_frameCount >= 2)
            {
                _damageApplied = true;
                ApplyEffects();
            }
        }

        //Desvanece gradualmente
        float alpha = 1f - (_timer / Duration);
        Modulate = new Color(1, 1, 1, Mathf.Max(0, alpha));

        if (_timer >= Duration)
            QueueFree();
    }

    private void ApplyEffects()
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is IEnemy enemy)
            {
                //Daño
                enemy.Health.TakeDamage(Damage, body.GlobalPosition, GetTree(),body.GetInstanceId());

                //Aplica el estado de congelación con la intensidad configurada por las mejoras.
                enemy.ApplyStatusEffect(new FrozenEffect(SlowFactor, SlowDuration));

                //Si tiene chain y no ha rebotado aún, entonces busca al siguiente enemigo
                if (IsChaining && !_hasChained)
                    TryChain(body.GlobalPosition);
            }
        }
    }

    private void TryChain(Vector2 hitPosition)
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D chainTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (var node in enemies)
        {
            if (node is Node2D enemy)
            {
                //No encadena al mismo enemigo que acaba de golpear
                if (enemy.GlobalPosition.DistanceTo(hitPosition) < 10f) continue;

                float distance = hitPosition.DistanceTo(enemy.GlobalPosition);
                if (distance < nearestDistance && distance < 300f)
                {
                    nearestDistance = distance;
                    chainTarget = enemy;
                }
            }
        }

        if (chainTarget == null) return;

        _hasChained = true;

        //Crea un nuevo rayo desde el enemigo golpeado hacia el siguiente enemigo
        FrostRay chainRay = (FrostRay)Duplicate();
        chainRay._hasChained = true; //el rayo encadenado no vuelve a encadenar
        GetParent().AddChild(chainRay);
        chainRay.GlobalPosition = hitPosition;

        Vector2 chainDirection = hitPosition.DirectionTo(chainTarget.GlobalPosition);
        chainRay.Rotation = chainDirection.Angle();
    }
}
