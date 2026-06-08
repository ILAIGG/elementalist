using Godot;

public partial class MainMenu : Control
{
    private Button _playButton;
    private Button _endlessModeButton;
    private Button _quitButton;

    public override void _Ready()
    {
        _playButton = GetNode<Button>("PlayButton");
        _endlessModeButton = GetNode<Button>("EndlessModeButton");
        _quitButton = GetNode<Button>("QuitButton");

        _playButton.Pressed += OnPlayPressed;
        _quitButton.Pressed += OnQuitPressed;
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/UI/SaveSlotScreen.tscn");
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