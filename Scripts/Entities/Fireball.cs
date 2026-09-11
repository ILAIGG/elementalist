using Godot;

public partial class Fireball : Area2D
{
    [Export] public Element ElementType { get; set; } = Element.Fire;
    [Export] public float Speed = 400f;
    [Export] public float Damage = 20f;
    [Export] public float ExplosionRadius = 80f;

    //La dirección a la que viaja el proyectil. Se asigna desde afuera al instanciar el proyectil.
    public Vector2 Direction { get; set; }
    public bool IsExplosive { get; set; }
    public PackedScene ExplosionEffectScene { get; set; }
    public bool IsPiercing { get; set; } = false;

    //Distancia máxima que puede recorrer antes de autodestruirse
    private const float MaxTravelDistance = 1000f;
    private float _distanceTraveled = 0f;

    public override void _Ready()
    {
        //Se conecta a la señal de Godot que se dispara cuando algo entra en el área del proyectil
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        //El proyectil se mueve hacia la dirección del enemigo cada frame
        float movement = Speed * (float)delta;
        _distanceTraveled += movement;
        Position += Direction * movement;

        //Se autodestruye si superó la distancia máxima
        if (_distanceTraveled >= MaxTravelDistance)
            QueueFree();
    }

    private void OnBodyEntered(Node2D body)
    {
        //Si toca a un enemigo, le hace daño
        if (body is IEnemy enemy)
        {
            enemy.TakeElementalDamage(Damage, ElementType, body.GlobalPosition, GetTree(), body.GetInstanceId());

            if (IsExplosive)
            {
                Explode();
                AudioManager.Instance.PlaySfx("sfx.explosion");
            }
            else if (!IsPiercing)
                QueueFree(); //Se elimina el proyectil al impactar solo si no es penetrante
            //Si es penetrante, entonces continúa sin destruirse
        }
    }

    private void Explode()
    {
        //Spawnea el efecto visual de la explosión
        if (ExplosionEffectScene != null)
        {
            ExplosionEffect effect = ExplosionEffectScene.Instantiate<ExplosionEffect>();
            effect.Radius = ExplosionRadius;
            GetTree().Root.FindChild("Projectiles", true, false).AddChild(effect);
            effect.GlobalPosition = GlobalPosition;
        }

        //Se daña a todos los enemigos dentro del radio de explosión
        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (var node in enemies)
        {
            if (node is Node2D enemy)
            {
                float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (distance <= ExplosionRadius)
                    (enemy as IEnemy)?.TakeElementalDamage(Damage * 0.5f, ElementType, enemy.GlobalPosition, GetTree(), enemy.GetInstanceId());
            }
        }

        QueueFree();
    }
}
