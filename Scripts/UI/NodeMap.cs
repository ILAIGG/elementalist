using Godot;
using System.Collections.Generic;

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
        _nodesContainer = GetNodeOrNull<Node2D>("Nodes");
        if (_nodesContainer == null)
        {
            _nodesContainer = new Node2D { Name = "Nodes" };
            AddChild(_nodesContainer);
        }

        _camera = GetNode<Camera2D>("Camera2D");
        _popUp = GetNode<Control>("PopUp");

        _pathsContainer = GetNodeOrNull<Node2D>("Paths");
        if (_pathsContainer == null)
        {
            _pathsContainer = new Node2D { Name = "Paths" };
            AddChild(_pathsContainer);
        }

        BuildGeneratedMap();
        DrawPaths();

        string popUpPath = "PopUp/Panel/Margin/Content";
        _nodeNameLabel = GetNode<Label>($"{popUpPath}/Header/NodeNameLabel");
        _difficultyLabel = GetNode<Label>($"{popUpPath}/DifficultyLabel");
        _enterButton = GetNode<Button>($"{popUpPath}/EnterButton");
        _closeButton = GetNode<Button>($"{popUpPath}/Header/CloseButton");

        _enterButton.Pressed += OnEnterPressed;
        _closeButton.Pressed += OnClosePressed;
        _popUp.ZIndex = 3;

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

    private void BuildGeneratedMap()
    {
        foreach (Node child in _nodesContainer.GetChildren())
            child.Free();

        foreach (Node child in _pathsContainer.GetChildren())
            child.Free();

        GameManager.Instance.EnsureRunMap();
        RunMapNode[] runMap = GameManager.Instance.CurrentRunMap;
        if (runMap.Length == 0)
        {
            GD.PushError("NodeMap could not build the run map: no generated nodes are available.");
            return;
        }

        PackedScene mapNodeScene = GD.Load<PackedScene>("res://Scenes/UI/MapNode.tscn");
        if (mapNodeScene == null)
        {
            GD.PushError("NodeMap could not load res://Scenes/UI/MapNode.tscn.");
            return;
        }

        foreach (RunMapNode data in runMap)
        {
            LevelDefinition level = data.IsTutorial ? null : LevelCatalog.GetLevel(data.LevelId);
            MapNode mapNode = mapNodeScene.Instantiate<MapNode>();
            mapNode.Name = $"MapNode{data.Id}";
            mapNode.ZIndex = 2;
            _nodesContainer.AddChild(mapNode);
            PackedScene levelScene = level?.Scene ?? GD.Load<PackedScene>("res://Scenes/World/Level_0_0.tscn");
            mapNode.Configure(data, levelScene, GetNodePosition(data, runMap));
        }

        _camera.LimitLeft = -500;
        _camera.LimitRight = 500;
        _camera.LimitTop = -600;
        _camera.LimitBottom = 300;
        _camera.Position = GameManager.Instance.GetNodeMapCameraPosition();
    }

    private Vector2 GetNodePosition(RunMapNode node, RunMapNode[] runMap)
    {
        if (node.IsTutorial)
            return new Vector2(0, 230);

        int layerNodeCount = 0;
        foreach (RunMapNode other in runMap)
        {
            if (other.Layer == node.Layer)
                layerNodeCount++;
        }

        float x = (node.Position - ((layerNodeCount - 1) / 2.0f)) * 180.0f;
        float y = 230.0f - (node.Layer * 130.0f);
        return new Vector2(x, y);
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

        //Posiciona el pop-up en coordenadas del mapa y lo mantiene dentro de la vista.
        Vector2 popupSize = _popUp.Size;
        Vector2 desiredPosition = node.Position - new Vector2(popupSize.X / 2, popupSize.Y + 20);
        Vector2 viewportSize = GetViewportRect().Size / _camera.Zoom;
        Vector2 viewTopLeft = _camera.Position - (viewportSize / 2);
        Vector2 viewBottomRight = _camera.Position + (viewportSize / 2);

        desiredPosition.X = Mathf.Clamp(
            desiredPosition.X,
            viewTopLeft.X + 10,
            viewBottomRight.X - popupSize.X - 10);
        desiredPosition.Y = Mathf.Clamp(
            desiredPosition.Y,
            viewTopLeft.Y + 10,
            viewBottomRight.Y - popupSize.Y - 10);

        _popUp.Position = desiredPosition;
        _popUp.Visible = true;
    }

    private void OnEnterPressed()
    {
        if (_selectedNode == null)
        {
            GD.PushError("Cannot enter level: no map node is selected.");
            return;
        }

        if (_selectedNode.LevelScene == null)
        {
            GD.PushError($"Cannot enter level: node {_selectedNode.NodeId} has no scene.");
            return;
        }

        //Guarda el nodo activo y su dificultad en el GameManager
        GameManager.Instance.ActiveNodeId = _selectedNode.NodeId;
        GameManager.Instance.ActiveNodeDifficulty = _selectedNode.DifficultyMultiplier;
        GameManager.Instance.ActiveNodeIsFinal = _selectedNode.IsFinal;
        GameManager.Instance.SaveNodeMapCameraPosition(_camera.Position);

        _popUp.Visible = false;
        CallDeferred(nameof(ChangeToSelectedLevel));
    }

    private void ChangeToSelectedLevel()
    {
        if (_selectedNode == null || _selectedNode.LevelScene == null)
            return;

        Error result = GetTree().ChangeSceneToPacked(_selectedNode.LevelScene);
        if (result != Error.Ok)
            GD.PushError($"Could not load level for node {_selectedNode.NodeId}: {result}");
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
        Dictionary<int, MapNode> mapNodes = new();

        foreach (Node child in _pathsContainer.GetChildren())
            child.Free();

        foreach (Node child in _nodesContainer.GetChildren())
        {
            if (child is MapNode mapNode)
                mapNodes[mapNode.NodeId] = mapNode;
        }

        foreach (MapNode source in mapNodes.Values)
        {
            foreach (int connectedId in source.ConnectedNodeIds)
            {
                if (!mapNodes.TryGetValue(connectedId, out MapNode target))
                    continue;

                Line2D line = new Line2D
                {
                    DefaultColor = pathColor,
                    Width = pathWidth,
                    ZIndex = 1
                };
                line.AddPoint(source.Position);
                line.AddPoint(target.Position);
                _pathsContainer.AddChild(line);
            }
        }
    }

    private void RefreshNodeStates()
    {
        RunMapNode currentNode = null;
        foreach (RunMapNode data in GameManager.Instance.CurrentRunMap)
        {
            if (data.Id == GameManager.Instance.CurrentRunNodeId)
            {
                currentNode = data;
                break;
            }
        }

        foreach (Node child in _nodesContainer.GetChildren())
        {
            if (child is MapNode mapNode)
            {
                bool unlocked = false;
                if (currentNode == null)
                {
                    unlocked = mapNode.NodeId == 0;
                }
                else
                {
                    foreach (int connectedId in currentNode.ConnectedNodeIds)
                    {
                        if (connectedId == mapNode.NodeId)
                        {
                            unlocked = true;
                            break;
                        }
                    }
                }

                if (GameManager.Instance.IsNodeCompleted(mapNode.NodeId))
                    unlocked = false;

                mapNode.SetState(unlocked, GameManager.Instance.IsNodeCompleted(mapNode.NodeId));
            }
        }
    }
}