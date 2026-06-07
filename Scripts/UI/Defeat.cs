using Godot;

public partial class Defeat : Control
{
    private Button _retryButton;
    private Button _returnToMapButton;
    private Button _mainMenuButton;

    public override void _Ready()
    {
        Visible = false;

        _retryButton = GetNode<Button>("UIContainer/Panel/Margin/Content/RetryButton");
        _returnToMapButton = GetNode<Button>("UIContainer/Panel/Margin/Content/ReturnToMapButton");
        _mainMenuButton = GetNode<Button>("UIContainer/Panel/Margin/Content/MainMenuButton");

        _retryButton.Pressed += OnRetryPressed;
        _returnToMapButton.Pressed += OnReturnToMapPressed;
        _mainMenuButton.Pressed += OnMainMenuPressed;
    }

    public void ShowDefeat()
    {
        Visible = true;
        GetTree().Paused = true;
    }

    private void OnRetryPressed()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    private void OnReturnToMapPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/World/NodeMap.tscn");
    }

    private void OnMainMenuPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}