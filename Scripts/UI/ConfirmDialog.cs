using Godot;
using System;

public partial class ConfirmDialog : Control
{
    private Label _title;
    private Button _confirmButton;
    private Button _cancelButton;

    //Evento que avisa cuando el jugador confirmó o canceló
    public event Action OnConfirmed;
    public event Action OnCancelled;

    public override void _Ready()
    {
        _title = GetNode<Label>("Panel/Margin/Content/Title");
        _confirmButton = GetNode<Button>("Panel/Margin/Content/Buttons/ConfirmButton");
        _cancelButton = GetNode<Button>("Panel/Margin/Content/Buttons/CancelButton");

        _confirmButton.Pressed += OnConfirmPressed;
        _cancelButton.Pressed += OnCancelPressed;
    }

    //Configura el texto del diálogo antes de mostrarlo. Si specificText es null o vacío, muestra el texto genérico
    public void SetMessage(string specificText = null)
    {
        //_Ready puede no haberse ejecutado aún si se llama antes de AddChild, por eso se busca el nodo directamente aquí también
        var title = GetNodeOrNull<Label>("Panel/Margin/Content/Title");
        if (title == null) return;

        if (string.IsNullOrEmpty(specificText))
            title.Text = "Are you sure?";
        else
            title.Text = $"Are you sure you want to {specificText}?";
    }

    private void OnConfirmPressed()
    {
        OnConfirmed?.Invoke();
        QueueFree();
    }

    private void OnCancelPressed()
    {
        OnCancelled?.Invoke();
        QueueFree();
    }
}