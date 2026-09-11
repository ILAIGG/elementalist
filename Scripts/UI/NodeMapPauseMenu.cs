using Godot;

public partial class NodeMapPauseMenu : Control
{
    private Button _resumeButton;
    private Button _settingsButton;
    private Button _saveAndQuitButton;

    private PackedScene _confirmDialog = GD.Load<PackedScene>("res://Scenes/UI/ConfirmDialog.tscn");

    public override void _Ready()
    {
        _resumeButton = GetNode<Button>("Panel/Margin/Content/ResumeButton");
        _settingsButton = GetNode<Button>("Panel/Margin/Content/SettingsButton");
        _saveAndQuitButton = GetNode<Button>("Panel/Margin/Content/SaveAndQuitButton");

        _resumeButton.Text = LocalizationManager.Translate("common.resume");
        _settingsButton.Text = LocalizationManager.Translate("menu.settings");
        _saveAndQuitButton.Text = LocalizationManager.Translate("common.save_and_quit");
        GetNode<Label>("Panel/Margin/Content/Title").Text = LocalizationManager.Translate("common.paused");

        _resumeButton.Pressed += OnResumePressed;
        _settingsButton.Pressed += OnSettingsPressed;
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
        dialog.SetMessage(LocalizationManager.Translate("confirm.save_to_menu"));

        dialog.OnConfirmed += () =>
        {
            GameManager.Instance.SaveGame();
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        };
    }

    private void OnSettingsPressed()
    {
        AddChild(GD.Load<PackedScene>("res://Scenes/UI/SettingsMenu.tscn").Instantiate());
    }
}