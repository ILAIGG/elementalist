using Godot;

public partial class Meteor : Area2D
{
    [Export] public Element ElementType { get; set; } = Element.Fire;
    [Export] public float Damage = 35f;
    [Export] public float FallSpeed = 800f;
    [Export] public PackedScene ExplosionEffectScene { get; set; }

    private bool _damageApplied = false;

    private int _frameCount = 0;

    //Posición final donde va a impactar
    private Vector2 _targetPosition;
    private bool _falling = true;

    public void SetTarget(Vector2 targetPosition, Vector2 cameraPosition)
    {
        _targetPosition = targetPosition;

        //El meteoro empieza 400 píxeles arriba de la pantalla relativo a la cámara actual
        GlobalPosition = new Vector2(
            targetPosition.X,
            cameraPosition.Y - 400f
        );
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_falling)
        {
            //Mueve el meteoro hacia su objetivo
            Vector2 direction = (_targetPosition - GlobalPosition).Normalized();
            GlobalPosition += direction * FallSpeed * (float)delta;

            //Cuando llega cerca del objetivo, impacta
            if (GlobalPosition.DistanceTo(_targetPosition) < 10f)
            {
                GlobalPosition = _targetPosition;
                _falling = false;
            }
        }
        else
        {
            //Ya llegó, espera un frame y aplica daño
            if (!_damageApplied)
            {
                _frameCount ++;

                //Espera 2 frames físicos antes de aplicar daño para garantizar que las colisiones estén procesadas
                if (_frameCount >= 2)
                {
                _damageApplied = true;
                ApplyDamage();
                }
            }
        }
    }

    private void ApplyDamage()
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is IEnemy enemy)
                enemy.TakeElementalDamage(Damage, ElementType, body.GlobalPosition, GetTree());
        }

        if (ExplosionEffectScene != null)
        {
            ExplosionEffect effect = ExplosionEffectScene.Instantiate<ExplosionEffect>();
            GetTree().Root.FindChild("Projectiles", true, false).AddChild(effect);
            effect.GlobalPosition = GlobalPosition;
        }

        //El meteoro desaparece después de impactar
        QueueFree();
    }
}
