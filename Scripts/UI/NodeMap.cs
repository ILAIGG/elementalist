using Godot;

public partial class NodeMap : Node2D
{
    private Node2D _nodesContainer;
    private Node2D _pathsContainer;
    private Control _popUp;
    private Label _nodeNameLabel;
    private Label _difficultyLabel;
    private Button _enterButton;
    private Button _closeButton;
    private Camera2D _camera;

    //Para el drag de la cámara
    private bool _isDragging = false;
    private Vector2 _dragStartPosition;
    private Vector2 _cameraStartPosition;

    //Nodo actualmente seleccionado
    private MapNode _selectedNode;

    //Menú de pausa
    private PackedScene _pauseMenuScene = GD.Load<PackedScene>("res://Scenes/UI/NodeMapPauseMenu.tscn");
    private bool _isPaused = false;
    private float _autosaveTimer = 0f;
    private const float AutosaveInterval = 30f;

    public override void _Ready()
    {
        _nodesContainer = GetNode<Node2D>("Nodes");
        _camera = GetNode<Camera2D>("Camera2D");
        _popUp = GetNode<Control>("PopUp");

        _pathsContainer = GetNode<Node2D>("Paths");
        DrawPaths();

        string popUpPath = "PopUp/Panel/Margin/Content";
        _nodeNameLabel = GetNode<Label>($"{popUpPath}/Header/NodeNameLabel");
        _difficultyLabel = GetNode<Label>($"{popUpPath}/DifficultyLabel");
        _enterButton = GetNode<Button>($"{popUpPath}/EnterButton");
        _closeButton = GetNode<Button>($"{popUpPath}/Header/CloseButton");

        _enterButton.Pressed += OnEnterPressed;
        _closeButton.Pressed += OnClosePressed;

        //Conecta todos los nodos hijos del contenedor
        foreach (Node child in _nodesContainer.GetChildren())
        {
            if (child is MapNode mapNode)
            {
                mapNode.OnNodeClicked += OnNodeClicked;
            }
        }
        RefreshNodeStates();

        _popUp.Visible = false;
    }

    public override void _Process(double delta)
    {
        //Autosave cada 30 segundos
        _autosaveTimer += (float)delta;
        if (_autosaveTimer >= AutosaveInterval)
        {
            _autosaveTimer = 0f;
            GameManager.Instance.SaveGame();
            GD.Print("Autosave realizado");
        }
    }

    public override void _Input(InputEvent @event)
    {
        //Drag de la cámara con click izquierdo
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    _isDragging = true;
                    _dragStartPosition = mouseButton.Position;
                    _cameraStartPosition = _camera.Position;
                }
                else
                {
                    _isDragging = false;
                }
            }
        }

        if (@event is InputEventMouseMotion mouseMotion && _isDragging)
        {
            Vector2 delta = mouseMotion.Position - _dragStartPosition;
            _camera.Position = _cameraStartPosition - delta;
        }

        if (@event.IsActionPressed("ui_cancel") && !_isPaused)
        {
            _isPaused = true;
            GetTree().Paused = true;

            NodeMapPauseMenu pauseMenu = _pauseMenuScene.Instantiate<NodeMapPauseMenu>();
            GetNode<CanvasLayer>("UI").AddChild(pauseMenu);

            //Cuando el menú se destruya, resetea _isPaused
            pauseMenu.TreeExited += () => _isPaused = false;
        }
    }

    private void OnNodeClicked(MapNode node)
    {
        _selectedNode = node;

        //Actualiza el pop-up con los datos del nodo
        _nodeNameLabel.Text = node.NodeName;
        _difficultyLabel.Text = $"Difficulty: x{node.DifficultyMultiplier}";

        //Posiciona el pop-up encima del nodo clickeado
        _popUp.Position = _camera.Position + node.Position - new Vector2(_popUp.Size.X / 2, _popUp.Size.Y + 20);
        _popUp.Visible = true;
    }

    private void OnEnterPressed()
    {
        if (_selectedNode == null || _selectedNode.LevelScene == null) return;

        //Guarda el nodo activo y su dificultad en el GameManager
        GameManager.Instance.ActiveNodeId = _selectedNode.NodeId;
        GameManager.Instance.ActiveNodeDifficulty = _selectedNode.DifficultyMultiplier;

        GetTree().ChangeSceneToPacked(_selectedNode.LevelScene);
    }

    private void OnClosePressed()
    {
        _popUp.Visible = false;
        _selectedNode = null;
    }

    private void DrawPaths()
    {
        Color pathColor = new Color(0.76f, 0.60f, 0.42f); // Marrón claro estilo tierra
        int pathWidth = 6;

        foreach (Node child in _pathsContainer.GetChildren())
        {
            if (child is Path2D path2D)
            {
                Line2D line = new Line2D();
                line.DefaultColor = pathColor;
                line.Width = pathWidth;

                //SampleBaked recorre la curva completa uniformemente
                Curve2D curve = path2D.Curve;
                float totalLength = curve.GetBakedLength();
                int sampleCount = 50;

                for (int i = 0; i <= sampleCount; i++)
                {
                    float distance = (float)i / sampleCount * totalLength;
                    Vector2 point = curve.SampleBaked(distance);
                    line.AddPoint(point + path2D.Position);
                }

                _pathsContainer.AddChild(line);
            }
        }
    }

    private void RefreshNodeStates()
    {
        foreach (Node child in _nodesContainer.GetChildren())
        {
            if (child is MapNode mapNode)
            {
                //El nodo 0 (tutorial) siempre está desbloqueado
                if (mapNode.NodeId == 0)
                {
                    mapNode.SetUnlocked(true);
                    continue;
                }

                //Un nodo se desbloquea si al menos uno de sus nodos prerequisito está completado — buscamos qué nodos apuntan a este nodo
                bool unlocked = false;
                foreach (Node other in _nodesContainer.GetChildren())
                {
                    if (other is MapNode otherNode)
                    {
                        foreach (int connectedId in otherNode.ConnectedNodeIds)
                        {
                            if (connectedId == mapNode.NodeId &&
                                GameManager.Instance.IsNodeCompleted(otherNode.NodeId))
                            {
                                unlocked = true;
                                break;
                            }
                        }
                    }

                    if (unlocked) break;
                }

                mapNode.SetUnlocked(unlocked);
            }
        }
    }
}