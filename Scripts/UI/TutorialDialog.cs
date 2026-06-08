using Godot;
using System.Collections.Generic;

public partial class TutorialDialog : CanvasLayer
{
    private ColorRect _overlay;
    private PanelContainer _dialogBox;
    private Label _dialogText;
    private Button _continueButton;

    private List<DialogStep> _steps = new();
    private int _currentStep = 0;

    public override void _Ready()
    {
        _overlay = GetNode<ColorRect>("Overlay");
        _dialogBox = GetNode<PanelContainer>("DialogBox");
        _dialogText = GetNode<Label>("DialogBox/Margin/Content/DialogText");
        _continueButton = GetNode<Button>("DialogBox/Margin/Content/ContinueButton");

        _continueButton.Pressed += OnContinuePressed;

        //Recopila todos los DialogStep hijos en orden
        foreach (Node child in GetChildren())
        {
            if (child is DialogStep step)
                _steps.Add(step);
        }

        if (_steps.Count > 0)
            ShowStep(0);
        else
            QueueFree();
    }

    private void ShowStep(int index)
    {
        if (index >= _steps.Count)
        {
            // Se terminaron los pasos
            GetTree().Paused = false;
            QueueFree();
            return;
        }

        DialogStep step = _steps[index];

        //Pausa el juego mientras se muestra el diálogo
        GetTree().Paused = true;

        //Actualiza el texto
        _dialogText.Text = step.Text;

        //Posiciona la caja de texto
        if (step.DialogPosition != Vector2.Zero)
            _dialogBox.Position = step.DialogPosition;
        else
        {
            //Centra la caja en pantalla
            Vector2 screenSize = GetViewport().GetVisibleRect().Size;
            _dialogBox.Position = (screenSize - _dialogBox.Size) / 2f;
        }

        //Muestra u oculta el highlight
        if (step.HighlightSize != Vector2.Zero)
            DrawHighlight(step.HighlightPosition, step.HighlightSize);
        else
            _overlay.Material = null;
    }

    private void DrawHighlight(Vector2 position, Vector2 size)
    {
        ShaderMaterial material = new ShaderMaterial();
        Shader shader = new Shader();
        shader.Code = @"
			shader_type canvas_item;
			uniform vec2 highlight_pos;
			uniform vec2 highlight_size;

			void fragment() {
				vec2 screen_pos = FRAGCOORD.xy;
				bool in_highlight = 
					screen_pos.x >= highlight_pos.x &&
					screen_pos.x <= highlight_pos.x + highlight_size.x &&
					screen_pos.y >= highlight_pos.y &&
					screen_pos.y <= highlight_pos.y + highlight_size.y;

				if (in_highlight)
					COLOR.a = 0.0;
				else
					COLOR.a = 0.8;
			}
		";
        material.Shader = shader;
        material.SetShaderParameter("highlight_pos", position);
        material.SetShaderParameter("highlight_size", size);

        _overlay.Material = material;
    }

    private void OnContinuePressed()
    {
        _currentStep++;
        ShowStep(_currentStep);
    }
}