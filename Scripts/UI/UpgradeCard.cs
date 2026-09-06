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

		//Se rellenan los labels con los datos del upgrade
		string localizedName = LocalizationManager.Translate(upgrade.NameKey);
		GetNode<Label>("CardContent/UpgradeName").Text = localizedName == upgrade.NameKey ? upgrade.Name : localizedName;
		GetNode<Label>("CardContent/UpgradeType").Text = LocalizationManager.Translate($"upgrade.type.{upgrade.Type.ToString().ToLowerInvariant()}");

		//Se muestra la descripción con el valor de la próxima aplicación
		int nextApplication = upgrade.TimesApplied + 1;
		string description = upgrade.GetDescription(nextApplication);
		string descriptionKey = upgrade.GetLocalizedDescriptionKey(nextApplication);
		string localizedDescription = LocalizationManager.Translate(
			descriptionKey,
			upgrade.GetLocalizedDescriptionArguments(nextApplication));
		GetNode<Label>("CardContent/UpgradeDescription").Text = localizedDescription == descriptionKey
			? description
			: localizedDescription;
	}

    public override void _GuiInput(InputEvent @event)
    {
        //Detecta cuando el jugador clickea la carta
		if (@event is InputEventMouseButton mb && mb.Pressed)
			OnUpgradeSelected?.Invoke(_upgrade);
    }
}
