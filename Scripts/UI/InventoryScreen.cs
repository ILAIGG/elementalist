using Godot;
using System.Collections.Generic;

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

        IReadOnlyList<Upgrade> upgrades = GetUpgradesForDisplay();
        foreach (var upgrade in upgrades)
        {
            UpgradeCard card = UpgradeCardScene.Instantiate<UpgradeCard>();
            container.AddChild(card);
            card.InitializeInventory(upgrade);
        }
    }

    private IReadOnlyList<Upgrade> GetUpgradesForDisplay()
    {
        // Si hay un jugador vivo en escena, usamos su sistema de upgrades directamente
        if (_player != null)
            return _player.Upgrades.AcquiredUpgrades;

        // En el mapa de nodos no hay jugador: reconstruimos la lista desde el save
        var save = GameManager.Instance?.ActiveSave;
        if (save == null || save.AcquiredUpgrades.Count == 0)
            return System.Array.Empty<Upgrade>();

        // Creamos un sistema de upgrades temporal sin jugador para poder leer los datos
        var tempUpgrades = new List<Upgrade>();
        var tempSystem = new UpgradeSystem(null, new PlayerStats());
        foreach (var kvp in save.AcquiredUpgrades)
        {
            var upgrade = tempSystem.FindUpgradeById(kvp.Key);
            if (upgrade != null)
            {
                upgrade.TimesApplied = kvp.Value;
                tempUpgrades.Add(upgrade);
            }
        }
        return tempUpgrades;
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