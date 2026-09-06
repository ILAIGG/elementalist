using Godot;

public partial class SaveSlotScreen : Control
{
    private Button _continueButton;
    private Button _newGameButton;
    private Button _deleteButton;
    private Label _runName;
    private Label _runDetails;
    private Button _backButton;

    private PackedScene _newGameDialog = GD.Load<PackedScene>("res://Scenes/UI/NewGameDialog.tscn");
    private PackedScene _confirmDialog = GD.Load<PackedScene>("res://Scenes/UI/ConfirmDialog.tscn");

    public override void _Ready()
    {
        _backButton = GetNode<Button>("SlotsContainer/BackButton");
        _backButton.Pressed += OnBackPressed;

        const string runPath = "SlotsContainer/Slot0/SlotMargin0/SlotContent0";
        _runName = GetNode<Label>($"{runPath}/SlotInfo0/SlotName0");
        _runDetails = GetNode<Label>($"{runPath}/SlotInfo0/SlotDetails0");
        _continueButton = GetNode<Button>($"{runPath}/SlotButtons0/ContinueButton0");
        _newGameButton = GetNode<Button>($"{runPath}/SlotButtons0/NewGameButton0");
        _deleteButton = GetNode<Button>($"{runPath}/SlotButtons0/DeleteButton0");

        _continueButton.Pressed += OnContinuePressed;
        _newGameButton.Pressed += OnNewGamePressed;
        _deleteButton.Pressed += OnDeletePressed;

        RefreshSlots();
    }

    //Actualiza la UI de la run guardada
    private void RefreshSlots()
    {
        if (SaveSystem.RunExists())
        {
            SaveData data = SaveSystem.LoadRun();
            _runName.Text = data.Name;
            _runDetails.Text = FormatPlaytime(data.PlaytimeSeconds);
            _continueButton.Visible = true;
            _newGameButton.Visible = false;
            _deleteButton.Visible = true;
            return;
        }

        _runName.Text = "No active run";
        _runDetails.Text = "Empty";
        _continueButton.Visible = false;
        _newGameButton.Visible = true;
        _deleteButton.Visible = false;
    }

    private string FormatPlaytime(float seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int remainingSeconds = (int)(seconds % 60);
        return $"Play time: {hours}h {minutes}m {remainingSeconds}s";
    }

    private void OnContinuePressed()
    {
        GameManager.Instance.LoadGame();
        GetTree().ChangeSceneToFile("res://Scenes/World/NodeMap.tscn");
    }

    private void OnNewGamePressed()
    {
        NewGameDialog dialog = _newGameDialog.Instantiate<NewGameDialog>();
        AddChild(dialog);

        dialog.OnConfirmed += (string saveName) =>
        {
            GameManager.Instance.NewGame(saveName);
            GetTree().ChangeSceneToFile("res://Scenes/World/NodeMap.tscn");
        };
    }

    private void OnDeletePressed()
    {
        ConfirmDialog dialog = _confirmDialog.Instantiate<ConfirmDialog>();
        AddChild(dialog);
        dialog.SetMessage($"delete \"{SaveSystem.LoadRun().Name}\"");

        dialog.OnConfirmed += () =>
        {
            SaveSystem.DeleteRun();
            RefreshSlots();
        };
    }

    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}