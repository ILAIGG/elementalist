using Godot;

public partial class SaveSlotScreen : Control
{
    private Button[] _continueButtons = new Button[3];
    private Button[] _newGameButtons = new Button[3];
    private Button[] _deleteButtons = new Button[3];
    private Label[] _slotNames = new Label[3];
    private Label[] _slotDetails = new Label[3];
    private Button _backButton;

    private PackedScene _newGameDialog = GD.Load<PackedScene>("res://Scenes/UI/NewGameDialog.tscn");
    private PackedScene _confirmDialog = GD.Load<PackedScene>("res://Scenes/UI/ConfirmDialog.tscn");

    public override void _Ready()
    {
        _backButton = GetNode<Button>("SlotsContainer/BackButton");
        _backButton.Pressed += OnBackPressed;

        for (int i = 0; i < 3; i++)
        {
            string slotPath = $"SlotsContainer/Slot{i}/SlotMargin{i}/SlotContent{i}";

            _slotNames[i] = GetNode<Label>($"{slotPath}/SlotInfo{i}/SlotName{i}");
            _slotDetails[i] = GetNode<Label>($"{slotPath}/SlotInfo{i}/SlotDetails{i}");
            _continueButtons[i] = GetNode<Button>($"{slotPath}/SlotButtons{i}/ContinueButton{i}");
            _newGameButtons[i] = GetNode<Button>($"{slotPath}/SlotButtons{i}/NewGameButton{i}");
            _deleteButtons[i] = GetNode<Button>($"{slotPath}/SlotButtons{i}/DeleteButton{i}");

            int slotIndex = i; //Captura para el lambda
            _continueButtons[i].Pressed += () => OnContinuePressed(slotIndex);
            _newGameButtons[i].Pressed += () => OnNewGamePressed(slotIndex);
            _deleteButtons[i].Pressed += () => OnDeletePressed(slotIndex);
        }

        RefreshSlots();
    }

    //Actualiza la UI de los tres slots según los datos guardados
    private void RefreshSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            if (SaveSystem.SlotExists(i))
            {
                SaveData data = SaveSystem.LoadSlot(i);
                _slotNames[i].Text = data.Name;
                _slotDetails[i].Text = FormatPlaytime(data.PlaytimeSeconds);
                _continueButtons[i].Visible = true;
                _newGameButtons[i].Visible = false;
                _deleteButtons[i].Visible = true;
            }
            else
            {
                _slotNames[i].Text = $"Slot {i + 1}";
                _slotDetails[i].Text = "Empty";
                _continueButtons[i].Visible = false;
                _newGameButtons[i].Visible = true;
                _deleteButtons[i].Visible = false;
            }
        }
    }

    private string FormatPlaytime(float seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)(seconds % 3600 / 60);
        return $"Play time: {hours}h {minutes}m {seconds}s";
    }

    private void OnContinuePressed(int slot)
    {
        GD.Print($"Continue");
        //Aquí se cargará la partida cuando se haga el mapa de nodos
    }

    private void OnNewGamePressed(int slot)
    {
        NewGameDialog dialog = _newGameDialog.Instantiate<NewGameDialog>();
        AddChild(dialog);

        dialog.OnConfirmed += (string saveName) =>
        {
            SaveData newData = new SaveData { Name = saveName };
            SaveSystem.SaveSlot(slot, newData);
            RefreshSlots();
        };
    }

    private void OnDeletePressed(int slot)
    {
        ConfirmDialog dialog = _confirmDialog.Instantiate<ConfirmDialog>();
        AddChild(dialog);
        dialog.SetMessage($"delete \"{SaveSystem.LoadSlot(slot).Name}\"");

        dialog.OnConfirmed += () =>
        {
            SaveSystem.DeleteSlot(slot);
            RefreshSlots();
        };
    }

    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}