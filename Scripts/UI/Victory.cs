using Godot;

public partial class Victory : Control
{
    private Button _returnToMapButton;

    public override void _Ready()
    {
        Visible = false;
        _returnToMapButton = GetNode<Button>("ReturnToMapButton");
        GetNode<Label>("Title").Text = LocalizationManager.Translate("screen.victory");
        _returnToMapButton.Text = LocalizationManager.Translate("common.return_to_map");
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