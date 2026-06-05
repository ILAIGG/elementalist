using Godot;

public partial class PauseMenu : Control
{
    private Button _resumeButton;
    private Button _returnToMapButton;
    private Button _mainMenuButton;

    public override void _Ready()
    {
        _resumeButton = GetNode<Button>("Panel/Margin/Content/ResumeButton");
        _returnToMapButton = GetNode<Button>("Panel/Margin/Content/ReturnToMapButton");
        _mainMenuButton = GetNode<Button>("Panel/Margin/Content/MainMenuButton");

        _resumeButton.Pressed += OnResumePressed;
        _returnToMapButton.Pressed += OnReturnToMapPressed;
        _mainMenuButton.Pressed += OnMainMenuPressed;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
            OnResumePressed();
    }

    private void OnResumePressed()
    {
        GetTree().Paused = false;
        QueueFree();
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