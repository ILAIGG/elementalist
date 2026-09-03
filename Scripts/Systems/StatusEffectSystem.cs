using Godot;

public sealed class StatusEffectSystem
{
    private float _slowFactor = 1f;
    private float _slowRemainingDuration = 0f;

    public float MovementFactor => _slowFactor;

    public void ApplySlow(float factor, float duration)
    {
        if (factor >= _slowFactor || duration <= 0f)
            return;

        _slowFactor = factor;
        _slowRemainingDuration = duration;
    }

    public void Update(double delta)
    {
        if (_slowRemainingDuration <= 0f)
            return;

        _slowRemainingDuration -= (float)delta;
        if (_slowRemainingDuration <= 0f)
            _slowFactor = 1f;
    }
}