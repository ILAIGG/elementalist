using Godot;

public partial class HowToPlay : Control
{
    public override void _Ready()
    {
        GetNode<Label>("Title").Text = LocalizationManager.Translate("howto.title");
        GetNode<Label>("Content/ControlsLabel").Text = LocalizationManager.Translate("howto.controls").Replace("\\n", "\n");
        GetNode<Label>("Content/AbilitiesLabel").Text = LocalizationManager.Translate("howto.upgrades").Replace("\\n", "\n");
        GetNode<Label>("Content/TipsLabel").Text = LocalizationManager.Translate("howto.objective").Replace("\\n", "\n");
        GetNode<Button>("CloseButton").Text = LocalizationManager.Translate("common.close");
        GetNode<Button>("CloseButton").Pressed += OnCloseButtonPressed;
    }

    private void OnCloseButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
