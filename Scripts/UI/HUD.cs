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
    private Button _inventoryButton;

    private Player _player;
    private InventoryScreen _inventoryScreen;
    private float _gameTime = 0f;
    public override void _Ready()
    {
        VBoxContainer _contentContainer = GetNode<VBoxContainer>("Container/PlayerInfoPanel/Margin/ContentContainer");

        //Se obtienen referencias a todos los nodos
        _healthBar = _contentContainer.GetNode<TextureProgressBar>("HealthBar");
        _healthLabel = _contentContainer.GetNode<Label>("HealthLabel");
        _xpBar = _contentContainer.GetNode<TextureProgressBar>("XPBar");
        _xpLabel = _contentContainer.GetNode<Label>("XPBar/XPLabel");
        _levelLabel = _contentContainer.GetNode<Label>("LevelLabel");
        _timeLabel = _contentContainer.GetNode<Label>("TimeLabel");
        _difficultyLabel = _contentContainer.GetNode<Label>("DifficultyLabel");
        _novaLabel = _contentContainer.GetNode<Label>("Cooldowns/NovaLabel");
        _meteorLabel = _contentContainer.GetNode<Label>("Cooldowns/MeteorLabel");
        _dashLabel = _contentContainer.GetNode<Label>("Cooldowns/DashLabel");
        _objectiveLabel = GetNode<Label>("Container/ObjectivePanel/Margin/ObjectiveLabel");
        _inventoryButton = GetNode<Button>("Container/InventoryButton");
        _inventoryButton.Pressed += OnInventoryButtonPressed;
        _inventoryScreen = GetNode<InventoryScreen>("InventoryScreen");

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
            _levelLabel.Text = LocalizationManager.Translate("hud.level", 1);
            UpdateTimeLabel();
            UpdateCooldownLabels();
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
        _difficultyLabel.Text = LocalizationManager.Translate("hud.difficulty", level);
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
        _levelLabel.Text = LocalizationManager.Translate("hud.level", newLevel);
    }

    private void UpdateTimeLabel()
    {
        int minutes = (int)(_gameTime / 60f);
        int seconds = (int)(_gameTime % 60f);
        _timeLabel.Text = LocalizationManager.Translate("hud.time", minutes, seconds);
    }

    private void UpdateCooldownLabels()
    {
        //Nova
        float novaCooldown = _player.AbilityManager.GetNovaCooldownRemaining();
        _novaLabel.Text = novaCooldown > 0
            ? LocalizationManager.Translate("hud.fire_nova_cooldown", novaCooldown)
            : LocalizationManager.Translate("hud.fire_nova_ready");

        //Meteoros
        float meteorCooldown = _player.AbilityManager.GetMeteorShowerCooldownRemaining();
        _meteorLabel.Text = meteorCooldown > 0
            ? LocalizationManager.Translate("hud.meteor_cooldown", meteorCooldown)
            : LocalizationManager.Translate("hud.meteor_ready");

        //Dash
        float dashCooldown = _player.GetDashCooldownRemaining();
        _dashLabel.Text = dashCooldown > 0
            ? LocalizationManager.Translate("hud.dash_cooldown", dashCooldown)
            : LocalizationManager.Translate("hud.dash_ready");
    }

    public void SetObjective(string objective)
    {
        _objectiveLabel.Text = LocalizationManager.Translate("hud.objective", objective);
    }

    public void UpdateObjectiveTime(float secondsRemaining)
    {
        int minutes = (int)(secondsRemaining / 60f);
        int seconds = (int)(secondsRemaining % 60f);
        _objectiveLabel.Text = LocalizationManager.Translate("hud.survive", minutes, seconds);
    }

    private void OnInventoryButtonPressed()
    {
        _inventoryScreen?.ShowInventory();
    }
}
