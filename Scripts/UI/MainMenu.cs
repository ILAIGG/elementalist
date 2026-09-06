using Godot;

public partial class MainMenu : Control
{
    private Button _playButton;
    private Button _continueButton;
    private Button _endlessModeButton;
    private Button _quitButton;
    private PackedScene _confirmDialog = GD.Load<PackedScene>("res://Scenes/UI/ConfirmDialog.tscn");

    public override void _Ready()
    {
        GameManager.Instance?.ClearActiveSave();
        _playButton = GetNode<Button>("PlayButton");
        _continueButton = GetNode<Button>("ContinueButton");
        _endlessModeButton = GetNode<Button>("EndlessModeButton");
        _quitButton = GetNode<Button>("QuitButton");

        _playButton.Pressed += OnPlayPressed;
        _continueButton.Pressed += OnContinuePressed;
        _quitButton.Pressed += OnQuitPressed;
        SaveData savedRun = SaveSystem.LoadRun();
        _continueButton.Disabled = savedRun == null || savedRun.IsRunComplete;
    }

    private void OnPlayPressed()
    {
        ConfirmDialog dialog = _confirmDialog.Instantiate<ConfirmDialog>();
        AddChild(dialog);
        dialog.SetMessage("start a new run? Your current run will be overwritten");

        dialog.OnConfirmed += () =>
        {
            GameManager.Instance.NewGame("Run");
            GetTree().ChangeSceneToFile("res://Scenes/World/NodeMap.tscn");
        };
    }

    private void OnContinuePressed()
    {
        ConfirmDialog dialog = _confirmDialog.Instantiate<ConfirmDialog>();
        AddChild(dialog);
        dialog.SetMessage("continue your current run?");

        dialog.OnConfirmed += () =>
        {
            GameManager.Instance.LoadGame();
            GetTree().ChangeSceneToFile("res://Scenes/World/NodeMap.tscn");
        };
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

#if DEBUG
    private Key[] _konamiCode = { Key.Up, Key.Up, Key.Down, Key.Down, Key.Left, Key.Right, Key.Left, Key.Right, Key.B, Key.A };
    private int _konamiIndex = 0;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == _konamiCode[_konamiIndex])
            {
                _konamiIndex++;
                if (_konamiIndex >= _konamiCode.Length)
                {
                    GameManager.Instance.GodModeEnabled = !GameManager.Instance.GodModeEnabled;
                    GD.Print("God Mode " + (GameManager.Instance.GodModeEnabled ? "Activated" : "Deactivated"));
                    _konamiIndex = 0;
                }
            }
            else
            {
                _konamiIndex = 0;
                if (keyEvent.Keycode == _konamiCode[0])
                    _konamiIndex = 1;
            }
        }
    }
#endif
}