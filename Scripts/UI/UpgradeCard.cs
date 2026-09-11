using Godot;
using System;

public partial class UpgradeCard : PanelContainer
{
	//Evento que se dispara cuando el jugador clickea esta carta
	public event Action<Upgrade> OnUpgradeSelected;

	private Upgrade _upgrade;

	public void Initialize(Upgrade upgrade)
	{
		_upgrade = upgrade;

		string localizedName = LocalizationManager.Translate(upgrade.NameKey);
		GetNode<Label>("CardContent/UpgradeName").Text = localizedName;
		GetNode<Label>("CardContent/UpgradeType").Text = LocalizationManager.Translate($"upgrade.type.{upgrade.Type.ToString().ToLowerInvariant()}");
		GetNode<Label>("CardContent/UpgradeElement").Text = LocalizationManager.Translate($"upgrade.element.{upgrade.ElementType.ToString().ToLowerInvariant()}");

		int nextApplication = upgrade.TimesApplied + 1;
		string descriptionKey = upgrade.GetLocalizedDescriptionKey(nextApplication);
		string localizedDescription = LocalizationManager.Translate(
			descriptionKey,
			upgrade.GetLocalizedDescriptionArguments(nextApplication));
		GetNode<Label>("CardContent/UpgradeDescription").Text = localizedDescription;
	}

    public override void _GuiInput(InputEvent @event)
    {
        //Detecta cuando el jugador clickea la carta
		if (@event is InputEventMouseButton mb && mb.Pressed)
		{
			AudioManager.Instance?.PlaySfx("ui.click", true);
			OnUpgradeSelected?.Invoke(_upgrade);
		}
    }
}
