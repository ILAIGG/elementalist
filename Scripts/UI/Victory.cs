using Godot;

public partial class Victory : Control
{
    private Button _returnToMapButton;

    public override void _Ready()
    {
        Visible = false;
        _returnToMapButton = GetNode<Button>("ReturnToMapButton");
        _returnToMapButton.Pressed += OnReturnToMapPressed;
    }

    public void ShowVictory()
    {
        Visible = true;
        GetTree().Paused = true;
    }

    private void OnReturnToMapPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/World/NodeMap.tscn");
    }
}