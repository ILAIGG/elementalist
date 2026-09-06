using System.Diagnostics.Metrics;

public class SaveData
{
    public string Name { get; set; } = "New Save";
    public float PlaytimeSeconds { get; set; } = 0f;
    public int[] CompletedNodes { get; set; } = System.Array.Empty<int>();
    public int MapSeed { get; set; }
    public int MapVersion { get; set; } = 1;
}
