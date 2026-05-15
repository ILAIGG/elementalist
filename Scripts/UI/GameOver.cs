using Godot;

public partial class GameOver : Control
{
    public override void _Ready()
    {
        //La pantalla empieza invisible
        Visible = false;

        //Se conecta el botón de reintentar;
        GetNode<Button>("RestartButton").Pressed += OnRestartPressed;
        GetNode<Button>("MenuButton").Pressed += OnMenuButtonPressed;
    }

    public void ShowGameOver()
    {
        Visible = true;

        //Se pausa el juego para que los enemigos y proyectiles dejen de moverse
        GetTree().Paused = true;
    }

    private void OnRestartPressed()
    {
        //Se quita la pausa del juego antes de recargar.
        GetTree().Paused = false;

        //Se recarga la escena actual desde cero
        GetTree().ReloadCurrentScene();
    }

    private void OnMenuButtonPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
