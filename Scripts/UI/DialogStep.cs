using Godot;

public partial class DialogStep : Node
{
    [Export] public string Text { get; set; } = "";

    public enum AnchorType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center,
        TopCenter,
        BottomCenter
    }

    // Resolución base del proyecto para calcular posiciones relativas
    [Export] public Vector2 BaseResolution { get; set; } = new Vector2(1152, 648);

    [ExportGroup("Highlight")]
    // Posición y tamaño del highlight (en coordenadas de pantalla de la resolución base)
    // Si HighlightSize es Vector2.Zero, no se muestra highlight
    [Export] public Vector2 HighlightPosition { get; set; } = Vector2.Zero;
    [Export] public Vector2 HighlightSize { get; set; } = Vector2.Zero;
    [Export] public AnchorType HighlightAnchor { get; set; } = AnchorType.TopLeft;

    [ExportGroup("Dialog Box")]
    // Posición de la caja de texto (en coordenadas de pantalla de la resolución base)
    // Si es Vector2.Zero, se centra en pantalla
    [Export] public Vector2 DialogPosition { get; set; } = Vector2.Zero;
    [Export] public AnchorType DialogAnchor { get; set; } = AnchorType.TopLeft;

    public Vector2 GetHighlightPosition(Vector2 currentScreenSize)
    {
        if (HighlightSize == Vector2.Zero) return Vector2.Zero;
        return AdjustPosition(HighlightPosition, HighlightAnchor, currentScreenSize);
    }

    public Vector2 GetDialogPosition(Vector2 currentScreenSize)
    {
        if (DialogPosition == Vector2.Zero) return Vector2.Zero;
        return AdjustPosition(DialogPosition, DialogAnchor, currentScreenSize);
    }

    private Vector2 AdjustPosition(Vector2 originalPos, AnchorType anchor, Vector2 screen)
    {
        Vector2 pos = originalPos;
        Vector2 baseRes = BaseResolution;

        switch (anchor)
        {
            case AnchorType.TopRight:
                pos.X = screen.X - (baseRes.X - originalPos.X);
                break;
            case AnchorType.BottomLeft:
                pos.Y = screen.Y - (baseRes.Y - originalPos.Y);
                break;
            case AnchorType.BottomRight:
                pos.X = screen.X - (baseRes.X - originalPos.X);
                pos.Y = screen.Y - (baseRes.Y - originalPos.Y);
                break;
            case AnchorType.Center:
                pos.X = screen.X / 2f - (baseRes.X / 2f - originalPos.X);
                pos.Y = screen.Y / 2f - (baseRes.Y / 2f - originalPos.Y);
                break;
            case AnchorType.TopCenter:
                pos.X = screen.X / 2f - (baseRes.X / 2f - originalPos.X);
                break;
            case AnchorType.BottomCenter:
                pos.X = screen.X / 2f - (baseRes.X / 2f - originalPos.X);
                pos.Y = screen.Y - (baseRes.Y - originalPos.Y);
                break;
        }
        return pos;
    }
}