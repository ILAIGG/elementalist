using Godot;

//Clase base abstracta para todas las condiciones de victoria
//Hereda de Node para poder ser agregada como nodo hijo en el editor
public abstract partial class VictoryCondition : Node
{
    [Export] public bool Enabled { get; set; } = true;

    public abstract bool IsCompleted { get; }
}