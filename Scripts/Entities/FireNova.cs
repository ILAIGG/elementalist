using Godot;

public partial class FireNova : Area2D
{
    [Export] public Element ElementType { get; set; } = Element.Fire;
    [Export] public float Damage = 40f;
    [Export] public float Duration = 0.3f; //Segundos que dura visualmente
    [Export] public float Radius = 150f;

    private float _timer = 0f;
    private bool _damageApplied = false;

    public override void _PhysicsProcess(double delta)
    {
        _timer += (float)delta;

        //Se aplica el daño solo una vez al aparecer
        if (!_damageApplied)
        {
            _damageApplied = true;
            ApplyDamage();
        }

        //Se actualiza el alpha y se pide redibujar
        QueueRedraw();

        //Cuando termina la duración, se elimina el nodo
        if (_timer >= Duration)
            QueueFree();
    }

    public override void _Draw()
    {
        //El alpha se reduce a medida que pasa el tiempo de la habilidad
        float alpha = 1f - (_timer / Duration);
        Color fillColor = new(1f, 0.27f, 0f, Mathf.Max(0, alpha) * 0.5f);
        Color borderColor = new(1f, 0.5f, 0f, Mathf.Max(0, alpha));

        //Círculo relleno semitransparente
        DrawCircle(Vector2.Zero, Radius, fillColor);

        //Borde del círculo más opaco para que se vea el rango más claramente
        DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 64, borderColor, 2f);
    }

    private void ApplyDamage()
    {
        //GetOverlappingBodies devuelve todos los cuerpos físicos dentro del área de colisión en este momento
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is IEnemy enemy)
                enemy.TakeElementalDamage(Damage, ElementType, body.GlobalPosition, GetTree());
                
        }
    }
}
