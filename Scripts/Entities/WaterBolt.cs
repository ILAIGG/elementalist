using Godot;

public partial class WaterBolt : Area2D
{
    [Export] public Element ElementType { get; set; } = Element.Water;
    [Export] public float Speed = 400f;
    [Export] public float Damage = 6f;

    public Vector2 Direction { get; set; }
    public bool IsPiercing { get; set; } = false;

    private const float MaxTravelDistance = 1000f;
    private float _distanceTraveled = 0f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        float movement = Speed * (float)delta;
        _distanceTraveled += movement;
        Position += Direction * movement;

        if (_distanceTraveled >= MaxTravelDistance)
            QueueFree();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is IEnemy enemy)
        {
            enemy.TakeElementalDamage(Damage, ElementType, body.GlobalPosition, GetTree(), body.GetInstanceId());

            if (!IsPiercing)
                QueueFree();
        }
    }
}
