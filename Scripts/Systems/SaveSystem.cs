using Godot;
using System.Text.Json;

public static class SaveSystem
{
    private const int SlotCount = 3;
    private const string SaveFolder = "user://saves/";

    //Devuelve el path del archivo para un slot dado (0, 1, 2)
    private static string GetSlotPath(int slot)
    {
        return $"{SaveFolder}slot_{slot}.json";
    }

    //Crea la carpeta save si ésta no existe
    private static void EnsureSaveFolderExists()
    {
        DirAccess dir = DirAccess.Open("user://");
        if (!dir.DirExists("saves"))
            dir.MakeDir("saves");
    }

    //Guarda los datos de un slot
    public static void SaveSlot(int slot, SaveData data)
    {
        EnsureSaveFolderExists();
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        FileAccess file = FileAccess.Open(GetSlotPath(slot), FileAccess.ModeFlags.Write);
        file.StoreString(json);
        file.Close();
    }

    //Carga los datos de un slot (devuelve null si no existe)
    public static SaveData LoadSlot(int slot)
    {
        string path = GetSlotPath(slot);
        if (!FileAccess.FileExists(path))
            return null;

        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        file.Close();

        return JsonSerializer.Deserialize<SaveData>(json);
    }

    //Elimina el archivo de un slot
    public static void DeleteSlot(int slot)
    {
        string path = GetSlotPath(slot);
        if (FileAccess.FileExists(path))
        {
            DirAccess dir = DirAccess.Open(SaveFolder);
            dir.Remove($"slot_{slot}.json");
        }
    }

    //Devuelve true si un slot tiene datos guardados
    public static bool SlotExists(int slot)
    {
        return FileAccess.FileExists(GetSlotPath(slot));
    }
}
