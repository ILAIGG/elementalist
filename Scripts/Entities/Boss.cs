using System.Security.Cryptography.X509Certificates;
using Godot;

public partial class Boss : CharacterBody2D, IEnemy
{
    [Export] public float Speed = 160f;
    [Export] public float MaxHealth = 200000f;
    [Export] public float ContactDamage = 80f;
    private float _damageCooldown = 0f;
    
    private const float DamageInterval = 0.5f;

    private Sprite2D _sprite;

    public HealthSystem Health { get; private set; }

    private Player _player;
    private int _currentPhase = 1;
    private ColorRect _colorRect;

    private float _slowFactor = 1f;
    private float _slowTimer = 0f;
    //Factor mínimo de slow que se puede aplicar al jefe, 0.4 significa que nunca irá a menos del 40% de su velocidad
    private const float MinSlowFactor = 0.4f;

    //Variables para el knockback
    private Vector2 _knockbackVelocity = Vector2.Zero;
    private const float KnockbackDecay = 8f; //Que tan rápido se frena

    public override void _Ready()
    {
        Health = new(MaxHealth);
        Health.OnDeath += OnBossDeath;
        Health.OnHealthChanged += OnHealthChanged;

        _sprite = GetNode<Sprite2D>("Sprite2D");

        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        _colorRect = GetNode<ColorRect>("ColorRect");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null) return;

        if (_knockbackVelocity != Vector2.Zero)
        {
            Velocity = _knockbackVelocity;
            _knockbackVelocity = _knockbackVelocity.MoveToward(Vector2.Zero, KnockbackDecay * Speed * (float)delta);
            MoveAndSlide();
            return;
        }

        if (_slowTimer > 0f)
        {
            _slowTimer -= (float)delta;
            if (_slowTimer <= 0f)
                _slowFactor = 1f;
        }

        if (_damageCooldown > 0f)
            _damageCooldown -= (float)delta;

        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();

        // Voltear sprite según dirección horizontal
        if (direction.X != 0)
            _sprite.FlipH = direction.X < 0;

        Velocity = direction * Speed * _slowFactor;
        MoveAndSlide();

        bool touchingPlayer = false;
        //Daño por contacto
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision2D collision = GetSlideCollision(i);
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

        //Si se pierde el contacto, se resetea el cooldown para el próximo impacto
        if (!touchingPlayer)
        {
            _damageCooldown = 0f;
        }
    }

    public void ApplySlow(float factor, float duration)
    {
        float actualFactor = factor;

        //En fase 2, solo recibe el 20% del efecto del slow
        if (_currentPhase == 2)
        {
            float slowEffect = 1f - factor;
            actualFactor = 1f - (slowEffect * 0.2f);
        }

        //El jefe resiste el slow, nunca puede quedar completamente inmóvil
        float resistedFactor = Mathf.Max(actualFactor, MinSlowFactor);
        
        if (actualFactor < _slowFactor)
        {
            _slowFactor = resistedFactor;
            _slowTimer = duration;
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        //El jefe resiste el knockback, recibe solo el 20% de la fuerza
        _knockbackVelocity = force * 0.2f;
    }

    private void OnHealthChanged(float current, float max)
    {
        float percent = current / max;

        //Fase 2: al llegar al 50% de vida
        if (percent <= 0.5f && _currentPhase == 1)
        {
            _currentPhase = 2;
            EnterPhase2();
        }
    }

    private void EnterPhase2()
    {
        //Se vuelve más rápido y cambia de color
        Speed *= 1.5f;
        _colorRect.Color = new(1f, 0f, 0f); //rojo
    }

    private void OnBossDeath()
    {
        GetTree().Root.FindChild("World", true, false)
            ?.Call("OnBossDefeated");

            QueueFree();
    }
}
