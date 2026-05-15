using Godot;

public partial class Victory : Control
{
    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>("MenuButton").Pressed += OnMenuButtonPressed;
    }

    public void ShowVictory()
    {
        Visible = true;
        GetTree().Paused = true;
    }

    private void OnMenuButtonPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
