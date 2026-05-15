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
		GetNode<Label>("CardContent/UpgradeName").Text = upgrade.Name;
		GetNode<Label>("CardContent/UpgradeType").Text = upgrade.Type.ToString();

		//Se muestra la descripción con el valor de la próxima aplicación
		string description = upgrade.GetDescription(upgrade.TimesApplied + 1);
		GetNode<Label>("CardContent/UpgradeDescription").Text = description;
	}

    public override void _GuiInput(InputEvent @event)
    {
        //Detecta cuando el jugador clickea la carta
		if (@event is InputEventMouseButton mb && mb.Pressed)
			OnUpgradeSelected?.Invoke(_upgrade);
    }
}
