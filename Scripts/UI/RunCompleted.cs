using Godot;

public partial class RunCompleted : Control
{
    private Button _mainMenuButton;

    public override void _Ready()
    {
        Visible = false;
        _mainMenuButton = GetNode<Button>("UIContainer/Panel/Margin/Content/MainMenuButton");
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
