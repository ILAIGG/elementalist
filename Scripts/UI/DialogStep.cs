using Godot;

public partial class DialogStep : Node
{
    [Export] public string Text { get; set; } = "";

    //Posición y tamaño del highlight (en coordenadas de pantalla)
    //Si HighlightSize es Vector2.Zero, no se muestra highlight
    [Export] public Vector2 HighlightPosition { get; set; } = Vector2.Zero;
    [Export] public Vector2 HighlightSize { get; set; } = Vector2.Zero;

    //Posición de la caja de texto (en coordenadas de pantalla)
    //Si es Vector2.Zero, se centra en pantalla
    [Export] public Vector2 DialogPosition { get; set; } = Vector2.Zero;
}