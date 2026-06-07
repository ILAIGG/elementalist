using Godot;

public partial class NodeMapPauseMenu : Control
{
    private Button _resumeButton;
    private Button _saveAndQuitButton;

    public override void _Ready()
    {
        _resumeButton = GetNode<Button>("Panel/Margin/Content/ResumeButton");
        _saveAndQuitButton = GetNode<Button>("Panel/Margin/Content/SaveAndQuitButton");

        _resumeButton.Pressed += OnResumePressed;
        _saveAndQuitButton.Pressed += OnSaveAndQuitPressed;
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

    private void OnSaveAndQuitPressed()
    {
        GameManager.Instance.SaveGame();
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}