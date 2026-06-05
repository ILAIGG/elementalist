using Godot;

public partial class Level : Node2D
{
    [Export] public PackedScene DamageNumberScene { get; set; }

    //600 secs = 10 mins
    [Export] public float BossTimer = 600f;
    [Export] public PackedScene BossScene { get; set; }

    private float _gameTimer = 0f;
    private bool _bossSpawned = false;

    private PackedScene _pauseMenuScene = GD.Load<PackedScene>("res://Scenes/UI/PauseMenu.tscn");
    private bool _isPaused = false;

    public override void _Ready()
    {
        DamageNumberSystem.Initialize(DamageNumberScene);
    }

    public override void _Process(double delta)
    {
        if (_bossSpawned) return;

        _gameTimer += (float)delta;

        if (_gameTimer >= BossTimer)
        {
            _bossSpawned = true;
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        Node enemyContainer = GetTree().Root.FindChild("Enemies", true, false);
        Boss boss = BossScene.Instantiate<Boss>();
        enemyContainer.AddChild(boss);

        //El jefe aparece a 700 píxeles del jugador
        Player player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player != null)
            boss.GlobalPosition = player.GlobalPosition + new Vector2(700, 0);

        //Se pausa el spawner cuando aparece el Boss
        EnemySpawner spawner = GetTree().Root.FindChild("EnemySpawner", true, false) as EnemySpawner;
        if (spawner != null)
            spawner.IsPaused = true;
    }

    private void OnBossDefeated()
    {
        Victory victory = GetTree().Root.FindChild("Victory", true, false) as Victory;
        victory?.ShowVictory();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel") && !_isPaused)
        {
            _isPaused = true;
            GetTree().Paused = true;

            PauseMenu pauseMenu = _pauseMenuScene.Instantiate<PauseMenu>();

            //Lo agrega al CanvasLayer de UI para que se dibuje encima de todo
            GetNode<CanvasLayer>("UI").AddChild(pauseMenu);

            //Cuando el PauseMenu se destruya, resetea _isPaused
            pauseMenu.TreeExited += () => _isPaused = false;
        }
    }
}
