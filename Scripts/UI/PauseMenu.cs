using Godot;

public partial class PauseMenu : Control
{
    private Button _resumeButton;
    private Button _settingsButton;
    private Button _returnToMapButton;
    private Button _mainMenuButton;

    private PackedScene _confirmDialog = GD.Load<PackedScene>("res://Scenes/UI/ConfirmDialog.tscn");

    public override void _Ready()
    {
        _resumeButton = GetNode<Button>("Panel/Margin/Content/ResumeButton");
        _settingsButton = GetNode<Button>("Panel/Margin/Content/SettingsButton");
        _returnToMapButton = GetNode<Button>("Panel/Margin/Content/ReturnToMapButton");
        _mainMenuButton = GetNode<Button>("Panel/Margin/Content/MainMenuButton");

        _resumeButton.Text = LocalizationManager.Translate("common.resume");
        _settingsButton.Text = LocalizationManager.Translate("menu.settings");
        _returnToMapButton.Text = LocalizationManager.Translate("common.return_to_map");
        _mainMenuButton.Text = LocalizationManager.Translate("common.main_menu");
        GetNode<Label>("Panel/Margin/Content/Title").Text = LocalizationManager.Translate("common.paused");

        _resumeButton.Pressed += OnResumePressed;
        _settingsButton.Pressed += OnSettingsPressed;
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
        ConfirmDialog dialog = _confirmDialog.Instantiate<ConfirmDialog>();
        AddChild(dialog);
        dialog.SetMessage(LocalizationManager.Translate("confirm.return_to_map"));

        dialog.OnConfirmed += () =>
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/World/NodeMap.tscn");
        };
    }

    private void OnMainMenuPressed()
    {
        ConfirmDialog dialog = _confirmDialog.Instantiate<ConfirmDialog>();
        AddChild(dialog);
        dialog.SetMessage(LocalizationManager.Translate("confirm.return_to_menu"));

        dialog.OnConfirmed += () =>
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        };
    }

    private void OnSettingsPressed()
    {
        AddChild(GD.Load<PackedScene>("res://Scenes/UI/SettingsMenu.tscn").Instantiate());
    }
}