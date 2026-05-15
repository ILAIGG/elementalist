using Godot;

public partial class HowToPlay : Control
{
    public override void _Ready()
    {
        GetNode<Button>("CloseButton").Pressed += OnCloseButtonPressed;
    }

    private void OnCloseButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
