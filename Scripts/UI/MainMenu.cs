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
}