using Godot;
using System.Text.Json;

public static class SaveSystem
{
    private const string SaveFolder = "user://saves/";
    private const string RunFileName = "run.json";
    private const string LegacySlotFileName = "slot_0.json";

    private static string GetRunPath()
    {
        return $"{SaveFolder}{RunFileName}";
    }

    //Crea la carpeta save si ésta no existe
    private static void EnsureSaveFolderExists()
    {
        DirAccess dir = DirAccess.Open("user://");
        if (!dir.DirExists("saves"))
            dir.MakeDir("saves");
    }

    //Guarda los datos de la run actual
    public static void SaveRun(SaveData data)
    {
        EnsureSaveFolderExists();
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        FileAccess file = FileAccess.Open(GetRunPath(), FileAccess.ModeFlags.Write);
        file.StoreString(json);
        file.Close();
    }

    //Carga los datos de la run actual
    public static SaveData LoadRun()
    {
        string path = GetRunPath();
        if (!FileAccess.FileExists(path))
            MigrateLegacyRun();

        if (!FileAccess.FileExists(path))
            return null;

        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        file.Close();

        return JsonSerializer.Deserialize<SaveData>(json);
    }

    //Elimina la run actual
    public static void DeleteRun()
    {
        string path = GetRunPath();
        if (FileAccess.FileExists(path))
        {
            DirAccess dir = DirAccess.Open(SaveFolder);
            dir.Remove(RunFileName);
        }
    }

    //Devuelve true si existe una run guardada
    public static bool RunExists()
    {
        return FileAccess.FileExists(GetRunPath()) || FileAccess.FileExists($"{SaveFolder}{LegacySlotFileName}");
    }

    private static void MigrateLegacyRun()
    {
        string legacyPath = $"{SaveFolder}{LegacySlotFileName}";
        if (!FileAccess.FileExists(legacyPath))
            return;

        DirAccess dir = DirAccess.Open(SaveFolder);
        dir.Rename(LegacySlotFileName, RunFileName);
    }
}
