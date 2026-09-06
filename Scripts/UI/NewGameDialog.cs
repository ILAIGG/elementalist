using Godot;
using System;

public partial class NewGameDialog : Control
{
    private LineEdit _nameInput;
    private Button _confirmButton;
    private Button _cancelButton;

    //Evento que avisa al SaveSlotScreen cuando el jugador confirmó
    public event Action<string> OnConfirmed;

    public override void _Ready()
    {
        _nameInput = GetNode<LineEdit>("Panel/Margin/Content/NameInput");
        _confirmButton = GetNode<Button>("Panel/Margin/Content/Buttons/ConfirmButton");
        _cancelButton = GetNode<Button>("Panel/Margin/Content/Buttons/CancelButton");

        GetNode<Label>("Panel/Margin/Content/Title").Text = LocalizationManager.Translate("screen.new_save");
        _nameInput.PlaceholderText = LocalizationManager.Translate("screen.save_name_placeholder");
        _cancelButton.Text = LocalizationManager.Translate("common.cancel");
        _confirmButton.Text = LocalizationManager.Translate("common.confirm");

        _confirmButton.Pressed += OnConfirmPressed;
        _cancelButton.Pressed += OnCancelPressed;
    }

    private void OnConfirmPressed()
    {
        string name = _nameInput.Text.Trim();

        //Si el nombre está vacío, usa un nombre por defecto
        if (string.IsNullOrEmpty(name))
            name = "Unnamed";

        OnConfirmed?.Invoke(name);
        QueueFree();
    }

    private void OnCancelPressed()
    {
        QueueFree();
    }
}