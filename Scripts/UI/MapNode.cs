using Godot;
using System;

public partial class MapNode : Node2D
{
    [Export] public string NodeName { get; set; } = "Node";
    [Export] public float DifficultyMultiplier { get; set; } = 1.0f;
    [Export] public PackedScene LevelScene { get; set; }
    [Export] public int NodeId { get; set; } = 0;
    [Export] public int[] ConnectedNodeIds { get; set; } = Array.Empty<int>();

    private Button _nodeButton;
    private Label _nodeLabel;

    //Evento que avisa al mapa cuando este nodo fue clickeado
    public event Action<MapNode> OnNodeClicked;

    public override void _Ready()
    {
        _nodeButton = GetNode<Button>("NodeButton");
        _nodeLabel = GetNode<Label>("NodeLabel");

        _nodeButton.Pressed += () => OnNodeClicked?.Invoke(this);
        _nodeLabel.Text = NodeName;
    }

    //Configura el estado visual del nodo según si está desbloqueado o completado
    public void SetState(bool unlocked, bool completed)
    {
        _nodeButton.Disabled = !unlocked;

        if (completed)
            _nodeButton.Modulate = Colors.LightGreen;
        else if (unlocked)
            _nodeButton.Modulate = Colors.White;
        else
            _nodeButton.Modulate = new Color(0.4f, 0.4f, 0.4f);
    }
}