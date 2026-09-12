using Godot;

public partial class InventoryScreen : Control
{
    [Export] public PackedScene UpgradeCardScene { get; set; }
    private Button _closeButton;

    private Player _player;

    public override void _Ready()
    {
        Visible = false;

        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        _closeButton = GetNode<Button>("Panel/VBoxContainer/HBoxContainer/CloseButton");
        _closeButton.Pressed += OnCloseButtonPressed;
    }

    public void ShowInventory()
    {
        Visible = true;

        GetTree().Paused = true;
        AudioManager.Instance?.SetAudioPaused(true);

        RefreshInventory();
    }

    private void RefreshInventory()
    {
        var container =
            GetNode<GridContainer>("Panel/VBoxContainer/ScrollContainer/UpgradesContainer");

        foreach (Node child in container.GetChildren())
            child.QueueFree();

        foreach (var upgrade in _player.Upgrades.AcquiredUpgrades)
        {
            UpgradeCard card =
                UpgradeCardScene.Instantiate<UpgradeCard>();

            container.AddChild(card);

            card.InitializeInventory(upgrade);
        }
    }

    public void HideInventory()
    {
        Visible = false;

        GetTree().Paused = false;
        AudioManager.Instance?.SetAudioPaused(false);
    }

    private void OnCloseButtonPressed()
    {
        HideInventory();
    }
}