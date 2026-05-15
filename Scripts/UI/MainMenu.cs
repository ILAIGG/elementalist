using Godot;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        GetNode<Button>("PlayButton").Pressed += OnPlayButtonPressed;
        GetNode<Button>("HowToPlayButton").Pressed += OnHowToPlayButtonPressed;
        GetNode<Button>("QuitButton").Pressed += OnQuitButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
    }

    private void OnHowToPlayButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/UI/HowToPlay.tscn");
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
