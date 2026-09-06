using System;

public class RunMapNode
{
    public int Id { get; set; }
    public int Layer { get; set; }
    public int Position { get; set; }
    public string LevelId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public float DifficultyMultiplier { get; set; }
    public int[] ConnectedNodeIds { get; set; } = Array.Empty<int>();
    public bool IsTutorial { get; set; }
    public bool IsFinal { get; set; }
}
