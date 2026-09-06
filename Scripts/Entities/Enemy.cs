using System.Timers;
using Godot;

public partial class Enemy : CharacterBody2D, IEnemy
{
    private static readonly Color FrozenTint = new(0.72f, 0.9f, 1f);

    [Export] public float Speed = 80f;
    [Export] public float MaxHealth = 30f;

    private Sprite2D _sprite;

    //Variables para el cooldown de daño
    private float _damageCooldown = 0f;
    private const float DamageInterval = 0.5f; //Cada 0.5 segundos
    private const float ContactDamage = 8f; //Daño por impacto

    //La XP que da este enemigo al morir. Se usa MaxHealth como base para que los enemigos más fuertes den aún más XP automáticamente.
    public float XPValue => 10 * (MaxHealth / 30f);

    public HealthSystem Health { get; private set; }

    //Una referencia al jugador, la usamos para saber hacia donde debe moverse el enemigo. No usamos export ya que sino tendríamos que referenciar al jugador desde el inspector, lo cual es tedioso.
    private Player _player;

    private readonly StatusEffectSystem _statusEffects = new();

    //Variables para el knockback
    private Vector2 _knockbackVelocity = Vector2.Zero;
    private const float KnockbackDecay = 8f; //Que tan rápido se frena

    public override void _Ready()
    {
        Health = new HealthSystem(MaxHealth);

        _sprite = GetNode<Sprite2D>("Sprite2D");

        //Se busca al jugador en la escena usando su nombre. El % es un atajo de Godot para buscar un nodo
        //por un nombre en toda la escena.
        _player = GetTree().GetFirstNodeInGroup("player") as Player;

        //Cuando el enemigo muere, se elimina el nodo de la escena
        Health.OnDeath += OnEnemyDeath;
    }

    public override void _PhysicsProcess(double delta)
    {
        _statusEffects.Update(delta);
        UpdateStatusEffectVisuals();

        if (_player == null) return;

        //Se aplica el knockback y se reduce gradualmente
        if (_knockbackVelocity != Vector2.Zero)
        {
            Velocity = _knockbackVelocity;
            _knockbackVelocity = _knockbackVelocity.MoveToward(Vector2.Zero, KnockbackDecay * Speed * (float)delta);
            MoveAndSlide();
            return; //Mientras no haya knockback, se ignora el movimiento normal
        }

        //Se reduce el cooldown de daño
        if (_damageCooldown > 0f)
            _damageCooldown -= (float)delta;

        //Se calcula la dirección desde el enemigo hasta el jugador. Simplemente se le resta la posición del enemigo a la posición del jugador
        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();

        //Voltea sprite según dirección horizontal
        if (direction.X != 0)
            _sprite.FlipH = direction.X < 0;

        Velocity = direction * Speed * _statusEffects.MovementFactor;
        MoveAndSlide();

        bool touchingPlayer = false;
        //Después de moverse, verifica que haya tocado al jugador
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision2D collision = GetSlideCollision(i);

            //Verificamos si el objeto con el que chocamos fue el jugador
            if (collision.GetCollider() is Player player)
            {
                touchingPlayer = true;
                if (_damageCooldown <= 0f)
                {
                    player.Health.TakeDamage(ContactDamage, player.GlobalPosition, GetTree(), GetInstanceId());
                    _damageCooldown = DamageInterval;
                }
            }
        }

        //Si se pierde el contacto, el cooldown se resetea a 0 para el próximo impacto
        if (!touchingPlayer)
        {
            _damageCooldown = 0f;
        }
    }

    public void ScaleStats(float healthMultiplier, float speedMultiplier)
    {
        //Escala la vida y velocidad por los multiplicadores
        MaxHealth *= healthMultiplier;
        Health.SetMaxHealth(MaxHealth);
        Speed *= speedMultiplier;
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        _statusEffects.Apply(effect);
        UpdateStatusEffectVisuals();
    }

    private void UpdateStatusEffectVisuals()
    {
        _sprite.Modulate = _statusEffects.Has<FrozenEffect>() ? FrozenTint : Colors.White;
    }

    public void ApplyKnockback(Vector2 force)
    {
        _knockbackVelocity = force;
    }

    private void OnEnemyDeath()
    {
        //Se le da experiencia al jugador antes de eliminar al enemigo
        _player?.Experience.AddXP(XPValue); //Esto es lo mismo que hacer: "if (_player != null) _player.Experience.AddXP(XPValue);" El "?" hace que diga "Existe el jugador? Si existe, entonces haz esto".

        QueueFree();
    }
}
