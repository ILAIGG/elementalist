using Godot;

[GlobalClass]
public partial class AudioData : Resource
{
    [Export]
    public string Id { get; set; } = "";

    [Export]
    public AudioStream Stream { get; set; }

    [Export]
    public float VolumeDb { get; set; } = 0.0f;

    [Export]
    public float PitchScale { get; set; } = 1.0f;
}