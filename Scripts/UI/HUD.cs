using Godot;

public partial class HUD : CanvasLayer
{
    private TextureProgressBar _healthBar;
    private Label _healthLabel;
    private TextureProgressBar _xpBar;
    private Label _xpLabel;
    private Label _levelLabel;
    private Label _timeLabel;
    private Label _difficultyLabel;
    private Label _novaLabel;
    private Label _meteorLabel;
    private Label _dashLabel;
    private Label _objectiveLabel;

    private Player _player;
    private float _gameTime = 0f;
    public override void _Ready()
    {
        //Se obtienen referencias a todos los nodos
        _healthBar = GetNode<TextureProgressBar>("Container/HealthBar");
        _healthLabel = GetNode<Label>("Container/HealthLabel");
        _xpBar = GetNode<TextureProgressBar>("Container/XPBar");
        _xpLabel = GetNode<Label>("Container/XPBar/XPLabel");
        _levelLabel = GetNode<Label>("Container/LevelLabel");
        _timeLabel = GetNode<Label>("Container/TimeLabel");
        _difficultyLabel = GetNode<Label>("Container/DifficultyLabel");
        _novaLabel = GetNode<Label>("Container/Cooldowns/NovaLabel");
        _meteorLabel = GetNode<Label>("Container/Cooldowns/MeteorLabel");
        _dashLabel = GetNode<Label>("Container/Cooldowns/DashLabel");
        _objectiveLabel = GetNode<Label>("Container/ObjectiveLabel");

        _player = GetTree().GetFirstNodeInGroup("player") as Player;

        //Se suscribe a los eventos del jugador
        if (_player != null)
        {
            _player.Health.OnHealthChanged += OnHealthChanged;
            _player.Experience.OnXPChanged += OnXPChanged;
            _player.Experience.OnLevelUp += OnLevelUp;

            //Se inicializan todos los valores
            _healthBar.MaxValue = _player.Stats.MaxHealth;
            _healthBar.Value = _player.Stats.MaxHealth;
            _healthLabel.Text = $"{_player.Stats.MaxHealth} / {_player.Stats.MaxHealth}";
        }
    }

    public override void _Process(double delta)
    {
        if (_player == null) return;

        _gameTime += (float)delta;
        UpdateTimeLabel();
        UpdateCooldownLabels();
    }

    //Llamado desde EnemySpawner cuando sube la dificultad
    public void UpdateDifficulty(int level)
    {
        _difficultyLabel.Text = $"Difficulty: {level}";
    }

    private void OnHealthChanged(float current, float max)
    {
        _healthBar.MaxValue = max;
        _healthBar.Value = current;
        _healthLabel.Text = $"{Mathf.Round(current)} / {Mathf.Round(max)}";
    }

    private void OnXPChanged(float current, float max)
    {
        _xpBar.MaxValue = max;
        _xpBar.Value = current;
        _xpLabel.Text = $"{(int)current} / {(int)max}";
    }

    private void OnLevelUp(int newLevel)
    {
        _levelLabel.Text = $"Level {newLevel}";
    }

    private void UpdateTimeLabel()
    {
        int minutes = (int)(_gameTime / 60f);
        int seconds = (int)(_gameTime % 60f);
        _timeLabel.Text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void UpdateCooldownLabels()
    {
        //Nova
        float novaCooldown = _player.AbilityManager.GetNovaCooldownRemaining();
        _novaLabel.Text = novaCooldown > 0
            ? $"Fire Nova (Q): {novaCooldown:F1}s"
            : "Fire Nova (Q): Ready";

        //Meteoros
        float meteorCooldown = _player.AbilityManager.GetMeteorShowerCooldownRemaining();
        _meteorLabel.Text = meteorCooldown > 0
            ? $"Meteor Shower (E): {meteorCooldown:F1}s"
            : "Meteor Shower (E): Ready";

        //Dash
        float dashCooldown = _player.GetDashCooldownRemaining();
        _dashLabel.Text = dashCooldown > 0
            ? $"Dash (Space): {dashCooldown:F1}s"
            : "Dash (Space): Ready";
    }

    public void SetObjective(string objective)
    {
        _objectiveLabel.Text = $"Objective: {objective}";
    }

    public void UpdateObjectiveTime(float secondsRemaining)
    {
        int minutes = (int)(secondsRemaining / 60f);
        int seconds = (int)(secondsRemaining % 60f);
        _objectiveLabel.Text = $"Objective: Survive {minutes:00}:{seconds:00}";
    }
}
