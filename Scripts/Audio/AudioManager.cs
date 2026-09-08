using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    private const int SfxPlayerCount = 8;

    [Export]
    public AudioLibrary Library { get; set; }

    private AudioStreamPlayer _musicPlayer;
    private readonly List<AudioStreamPlayer> _sfxPlayers = new();

    private readonly Dictionary<string, AudioData> _sfxLibrary = new();
    private readonly Dictionary<string, AudioData> _musicLibrary = new();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        SetupMusicPlayer();
        SetupSfxPlayers();
        LoadLibrary();

        GD.Print("AudioManager iniciado.");
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    private void SetupMusicPlayer()
    {
        _musicPlayer = new AudioStreamPlayer
        {
            Bus = "Music"
        };

        AddChild(_musicPlayer);
    }

    private void SetupSfxPlayers()
    {
        for (int i = 0; i < SfxPlayerCount; i++)
        {
            var player = new AudioStreamPlayer
            {
                Bus = "SFX"
            };

            AddChild(player);
            _sfxPlayers.Add(player);
        }
    }

    private void LoadLibrary()
    {
        if (Library == null)
        {
            GD.PushWarning("AudioManager: No se asignó una AudioLibrary.");
            return;
        }

        foreach (var audio in Library.Sfx)
        {
            if (audio == null)
                continue;

            if (string.IsNullOrWhiteSpace(audio.Id))
            {
                GD.PushWarning("AudioManager: Se encontró un SFX sin ID.");
                continue;
            }

            if (audio.Stream == null)
            {
                GD.PushWarning($"AudioManager: El SFX '{audio.Id}' no tiene AudioStream.");
                continue;
            }

            if (_sfxLibrary.ContainsKey(audio.Id))
            {
                GD.PushWarning($"AudioManager: ID de SFX duplicado: '{audio.Id}'.");
                continue;
            }

            _sfxLibrary.Add(audio.Id, audio);
        }

        foreach (var audio in Library.Music)
        {
            if (audio == null)
                continue;

            if (string.IsNullOrWhiteSpace(audio.Id))
            {
                GD.PushWarning("AudioManager: Se encontró música sin ID.");
                continue;
            }

            if (audio.Stream == null)
            {
                GD.PushWarning($"AudioManager: La música '{audio.Id}' no tiene AudioStream.");
                continue;
            }

            if (_musicLibrary.ContainsKey(audio.Id))
            {
                GD.PushWarning($"AudioManager: ID de música duplicado: '{audio.Id}'.");
                continue;
            }

            _musicLibrary.Add(audio.Id, audio);
        }

        GD.Print(
            $"AudioLibrary cargada: {_sfxLibrary.Count} SFX, " +
            $"{_musicLibrary.Count} músicas."
        );
    }

    public void PlaySfx(string id)
    {
        if (!_sfxLibrary.TryGetValue(id, out var audio))
        {
            GD.PushWarning($"AudioManager: No se encontró el SFX '{id}'.");
            return;
        }

        foreach (var player in _sfxPlayers)
        {
            if (player.Playing)
                continue;

            player.Stream = audio.Stream;
            player.VolumeDb = audio.VolumeDb;
            player.PitchScale = audio.PitchScale;
            player.Play();

            return;
        }

        GD.PushWarning("AudioManager: Todos los reproductores SFX están ocupados.");
    }

    public void PlayMusic(string id)
    {
        if (!_musicLibrary.TryGetValue(id, out var audio))
        {
            GD.PushWarning($"AudioManager: No se encontró la música '{id}'.");
            return;
        }

        if (_musicPlayer.Stream == audio.Stream && _musicPlayer.Playing)
            return;

        _musicPlayer.Stream = audio.Stream;
        _musicPlayer.VolumeDb = audio.VolumeDb;
        _musicPlayer.PitchScale = audio.PitchScale;
        _musicPlayer.Play();
    }

    public void StopMusic()
    {
        _musicPlayer.Stop();
    }
}