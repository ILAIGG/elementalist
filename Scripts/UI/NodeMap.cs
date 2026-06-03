using Godot;

public partial class NodeMap : Node2D
{
    private Node2D _nodesContainer;
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

    public override void _Ready()
    {
        _nodesContainer = GetNode<Node2D>("Nodes");
        _camera = GetNode<Camera2D>("Camera2D");
        _popUp = GetNode<Control>("PopUp");

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
                mapNode.OnNodeClicked += OnNodeClicked;
        }

        _popUp.Visible = false;
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
        //Aquí se cargará la escena del nivel cuando se conecte con el sistema de guardado
        GD.Print($"Entrando al nodo: {_selectedNode.NodeName}");
    }

    private void OnClosePressed()
    {
        _popUp.Visible = false;
        _selectedNode = null;
    }
}