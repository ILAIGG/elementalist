using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    //Slot activo y datos de la partida actual
    public int ActiveSlot { get; private set; } = -1;
    public int ActiveNodeId { get; set; } = -1;
    public SaveData ActiveSave { get; private set; }

    //Dificultad
    public float ActiveNodeDifficulty { get; set; } = 1.0f;

#if DEBUG
    public bool GodModeEnabled { get; set; } = false;
#endif

    public override void _Ready()
    {
        Instance = this;
    }

    //Carga una partida existente
    public void LoadGame(int slot)
    {
        ActiveSlot = slot;
        ActiveSave = SaveSystem.LoadSlot(slot);
    }

    //Crea una nueva partida
    public void NewGame(int slot, string saveName)
    {
        ActiveSlot = slot;
        ActiveSave = new SaveData { Name = saveName };
        SaveSystem.SaveSlot(slot, ActiveSave);
    }

    //Guarda el estado actual
    public void SaveGame()
    {
        if (ActiveSlot == -1 || ActiveSave == null) return;
        SaveSystem.SaveSlot(ActiveSlot, ActiveSave);
    }

    //Marca un nodo como completado y guarda
    public void CompleteNode(int nodeId)
    {
        if (ActiveSave == null) return;

        //Verifica que el nodo no esté ya completado
        foreach (int id in ActiveSave.CompletedNodes)
            if (id == nodeId) return;

        //Agrega el nodo completado al array
        int[] newCompleted = new int[ActiveSave.CompletedNodes.Length + 1];
        ActiveSave.CompletedNodes.CopyTo(newCompleted, 0);
        newCompleted[newCompleted.Length - 1] = nodeId;
        ActiveSave.CompletedNodes = newCompleted;

        SaveGame();
    }

    //Devuelve true si un nodo está completado
    public bool IsNodeCompleted(int nodeId)
    {
        if (ActiveSave == null) return false;
        foreach (int id in ActiveSave.CompletedNodes)
            if (id == nodeId) return true;
        return false;
    }
}