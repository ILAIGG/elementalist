using Godot;

public partial class RunCompleted : Control
{
    private Button _mainMenuButton;

    public override void _Ready()
    {
        Visible = false;
        _mainMenuButton = GetNode<Button>("UIContainer/Panel/Margin/Content/MainMenuButton");
        GetNode<Label>("UIContainer/Title").Text = LocalizationManager.Translate("screen.run_completed");
        GetNode<Label>("UIContainer/Panel/Margin/Content/Message").Text = LocalizationManager.Translate("screen.run_completed_message");
        _mainMenuButton.Text = LocalizationManager.Translate("common.main_menu");
        _mainMenuButton.Pressed += OnMainMenuPressed;
    }

    public void ShowRunCompleted()
    {
        Visible = true;
        GetTree().Paused = true;
    }

    private void OnMainMenuPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
