using Godot;

public partial class ExplosionEffect : Node2D
{
    [Export] public float Radius = 80f;
    [Export] public float Duration = 0.4f;

    private float _timer = 0f;

    public override void _Process(double delta)
    {
        _timer += (float)delta;
        QueueRedraw();

        if (_timer >= Duration)
            QueueFree();
    }

    public override void _Draw()
    {
        float progress = _timer / Duration;
        float alpha = 1f - progress;

        //El círculo crece mientras se desvanece, dando la sensación de onda expansiva
        float currentRadius = Radius * (0.5f + progress * 0.5f);

        Color fillColor = new(1f, 0.1f, 0f, alpha * 0.4f);
        Color borderColor = new(1f, 0.3f, 0f, alpha);

        DrawCircle(Vector2.Zero, currentRadius, fillColor);
        DrawArc(Vector2.Zero, currentRadius, 0, Mathf.Tau, 64, borderColor, 2.5f);
    }
}
