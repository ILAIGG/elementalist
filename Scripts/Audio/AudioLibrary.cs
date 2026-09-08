using Godot;

[GlobalClass]
public partial class AudioLibrary : Resource
{
    [Export]
    public AudioData[] Sfx { get; set; } = [];

    [Export]
    public AudioData[] Music { get; set; } = [];
}