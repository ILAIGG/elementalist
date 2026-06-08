using Godot;

public partial class NodeMapPauseMenu : Control
{
    private Button _resumeButton;
    private Button _saveAndQuitButton;

    private PackedScene _confirmDialog = GD.Load<PackedScene>("res://Scenes/UI/ConfirmDialog.tscn");

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
        ConfirmDialog dialog = _confirmDialog.Instantiate<ConfirmDialog>();
        AddChild(dialog);
        dialog.SetMessage("save and quit to the main menu?");

        dialog.OnConfirmed += () =>
        {
            GameManager.Instance.SaveGame();
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        };
    }
}