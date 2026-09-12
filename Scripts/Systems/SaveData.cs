using System;

public class SaveData
{
    public string Name { get; set; } = "New Save";
    public float PlaytimeSeconds { get; set; } = 0f;
    public int[] CompletedNodes { get; set; } = System.Array.Empty<int>();
    public int MapSeed { get; set; }
    public int MapVersion { get; set; } = 1;
    public RunMapNode[] MapNodes { get; set; } = Array.Empty<RunMapNode>();
    public bool IsRunComplete { get; set; }
    public int CurrentNodeId { get; set; } = -1;
    public float CameraPositionX { get; set; }
    public float CameraPositionY { get; set; }
    public int PlayerLevel { get; set; } = 1;
    public float PlayerXP { get; set; } = 0f;
    public float PlayerCurrentHealth { get; set; } = -1f;
    public float PlayerDashCooldown { get; set; } = -1f;
    public System.Collections.Generic.Dictionary<string, int> AcquiredUpgrades { get; set; } = new();
}
