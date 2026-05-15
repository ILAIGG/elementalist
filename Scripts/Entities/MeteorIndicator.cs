using Godot;

public partial class MeteorIndicator : Node2D
{
    [Export] public float Radius = 100f;
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
        float alpha = 1f - (_timer / Duration);

        Color fillColor = new(0.5f, 0.5f, 0.5f, alpha * 0.3f);
        Color borderColor = new(0.8f, 0.8f, 0.8f, alpha);

        DrawCircle(Vector2.Zero, Radius, fillColor);
        DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 64, borderColor, 2f);
    }
}
