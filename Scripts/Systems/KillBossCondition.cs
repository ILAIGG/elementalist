using Godot;

public partial class KillBossCondition : VictoryCondition
{
    private bool _bossSpawned = false;
    private bool _completed = false;

    public override bool IsCompleted => _completed;

    public override void _Process(double delta)
    {
        if (_completed) return;

        Boss boss = GetTree().Root.FindChild("Boss", true, false) as Boss;

        if (boss == null && _bossSpawned)
            _completed = true;
    }

    public void NotifyBossSpawned()
    {
        _bossSpawned = true;
    }
}