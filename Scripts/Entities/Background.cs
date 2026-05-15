using Godot;
using System;

public partial class Background : Node2D
{
    [Export] public int GridSize = 100; //Tamaño de cada celda
    [Export] public int MapSize = 2500; //Tamaño del mapa
    [Export] public Color BackgroundColor = new(0.13f, 0.2f, 0.13f); //Verde oscuro
    [Export] public Color GridColor = new(0.17f, 0.26f, 0.17f); //Un verde un toque más claro para que se noten las líneas de los grids

    public override void _Draw()
    {
        //Dibuja el fondo
        DrawRect(new Rect2(-MapSize, -MapSize, MapSize * 2, MapSize * 2), BackgroundColor);

        //Se dibujan las líneas verticales de la grid
        for (int x = -MapSize; x <= MapSize; x += GridSize)
        {
            DrawLine(
                new Vector2(x, -MapSize),
                new Vector2(x, MapSize),
                GridColor,
                1f
            );
        }

        //Ahora se dibujan las lineas horizontales
        for (int y = -MapSize; y <= MapSize; y += GridSize)
        {
            DrawLine(
                new Vector2(-MapSize, y),
                new Vector2(MapSize, y),
                GridColor,
                1f
            );
        }
    }
}
