using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    private const int SfxPlayerCount = 8;
    private const int MaxConcurrentSfx = 4;
    private const ulong SameSfxCooldownMs = 80;
    private const float PausedMusicVolumeOffsetDb = -10.0f;
    private const float NormalMusicCutoffHz = 20000.0f;
    private const float PausedMusicCutoffHz = 900.0f;
    private const float MusicQuitPauseFadeDuration = 0.25f;
    private const float MusicCrossfadeDuration = 1.5f;

    [Export]
    public AudioLibrary Library { get; set; }

    private AudioStreamPlayer _musicPlayerA;
    private AudioStreamPlayer _musicPlayerB;

    private AudioStreamPlayer _activeMusicPlayer;
    private AudioStreamPlayer _inactiveMusicPlayer;

    private Tween _musicCrossfadeTween;

    private float _activeMusicBaseVolumeDb;
    private AudioEffectLowPassFilter _musicPauseFilter;
    private readonly List<AudioStreamPlayer> _sfxPlayers = new();
    private readonly Dictionary<AudioStreamPlayer, float> _sfxBaseVolumes = new();
    private readonly Dictionary<AudioStreamPlayer, ulong> _sfxStartTimes = new();
    private readonly Dictionary<string, ulong> _lastSfxPlayTimes = new();

    private readonly Dictionary<string, AudioData> _sfxLibrary = new();
    private readonly Dictionary<string, AudioData> _musicLibrary = new();
    private readonly HashSet<BaseButton> _uiButtons = new();
    private bool _audioWasPaused;
    private float _musicVolumeDb;
    private float _musicPauseProgress;
    private Tween _musicPauseTween;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;
        GetTree().NodeAdded += OnNodeAdded;
        ConnectExistingButtons(GetTree().Root);
        SetupMusicPlayer();
        SetupSfxPlayers();
        SetupSfxLimiter();
        LoadLibrary();

        GD.Print("AudioManager iniciado.");
    }

    public override void _Process(double delta)
    {
        UpdateAudioPauseState();
        UpdateSfxMix();
    }

    public override void _ExitTree()
    {
        GetTree().NodeAdded -= OnNodeAdded;

        if (Instance == this)
            Instance = null;
    }

    private void OnNodeAdded(Node node)
    {
        if (node is BaseButton button && _uiButtons.Add(button))
            button.Pressed += OnUiButtonPressed;
    }

    private void ConnectExistingButtons(Node node)
    {
        OnNodeAdded(node);

        foreach (Node child in node.GetChildren())
            ConnectExistingButtons(child);
    }

    private void OnUiButtonPressed()
    {
        AudioManager.Instance.PlaySfx("ui.click", true);
    }

    private void SetupMusicPlayer()
    {
        int busIndex = AudioServer.GetBusIndex("Music");

        if (busIndex != -1)
        {
            _musicVolumeDb = AudioServer.GetBusVolumeDb(busIndex);

            _musicPauseFilter = new AudioEffectLowPassFilter
            {
                CutoffHz = NormalMusicCutoffHz
            };

            AudioServer.AddBusEffect(busIndex, _musicPauseFilter);
        }

        _musicPlayerA = CreateMusicPlayer();
        _musicPlayerB = CreateMusicPlayer();

        _activeMusicPlayer = _musicPlayerA;
        _inactiveMusicPlayer = _musicPlayerB;
    }

    private AudioStreamPlayer CreateMusicPlayer()
    {
        var player = new AudioStreamPlayer
        {
            Bus = "Music",
            ProcessMode = Node.ProcessModeEnum.Pausable,
            VolumeDb = -80.0f
        };

        AddChild(player);

        return player;
    }

    private void SetupSfxPlayers()
    {
        for (int i = 0; i < SfxPlayerCount; i++)
        {
            var player = new AudioStreamPlayer
            {
                Bus = "SFX",
                ProcessMode = Node.ProcessModeEnum.Pausable
            };

            AddChild(player);
            _sfxPlayers.Add(player);
        }
    }

    private void SetupSfxLimiter()
    {
        int busIndex = AudioServer.GetBusIndex("SFX");
        if (busIndex == -1)
        {
            GD.PushWarning("AudioManager: No existe el bus 'SFX' para añadir el limitador.");
            return;
        }

        AudioServer.AddBusEffect(busIndex, new AudioEffectHardLimiter
        {
            CeilingDb = -1.0f
        });
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

    public void PlaySfx(string id, bool ignorePause = false)
    {
        if (GetTree().Paused && !ignorePause)
            return;

        if (!_sfxLibrary.TryGetValue(id, out var audio))
        {
            GD.PushWarning($"AudioManager: No se encontró el SFX '{id}'.");
            return;
        }

        ulong now = Time.GetTicksMsec();
        if (_lastSfxPlayTimes.TryGetValue(id, out ulong lastPlayTime) &&
            now - lastPlayTime < SameSfxCooldownMs)
        {
            return;
        }

        AudioStreamPlayer player = FindSfxPlayer();
        if (player == null)
            return;

        player.Stream = audio.Stream;
        _sfxBaseVolumes[player] = audio.VolumeDb;
        _sfxStartTimes[player] = now;
        _lastSfxPlayTimes[id] = now;
        player.PitchScale = audio.PitchScale;
        player.Play();
        UpdateSfxMix();
    }

    private void UpdateAudioPauseState()
    {
        SetAudioPaused(GetTree().Paused);
    }

    public void SetAudioPaused(bool shouldPauseAudio)
    {
        if (shouldPauseAudio == _audioWasPaused)
            return;

        _audioWasPaused = shouldPauseAudio;

        foreach (var player in _sfxPlayers)
            player.StreamPaused = shouldPauseAudio;

        if (_musicPlayerA != null)
            _musicPlayerA.StreamPaused = false;

        if (_musicPlayerB != null)
            _musicPlayerB.StreamPaused = false;

        TweenMusicPauseEffect(shouldPauseAudio);
    }

    private void TweenMusicPauseEffect(bool shouldPauseAudio)
    {
        _musicPauseTween?.Kill();

        float targetProgress = shouldPauseAudio ? 1.0f : 0.0f;
        _musicPauseTween = CreateTween();
        _musicPauseTween.SetPauseMode(Tween.TweenPauseMode.Process);
        _musicPauseTween.TweenMethod(
            Callable.From<float>(SetMusicPauseProgress),
            _musicPauseProgress,
            targetProgress,
            MusicQuitPauseFadeDuration
        );
    }

    private void SetMusicPauseProgress(float progress)
    {
        _musicPauseProgress = progress;

        int busIndex = AudioServer.GetBusIndex("Music");
        if (busIndex == -1)
            return;

        AudioServer.SetBusVolumeDb(
            busIndex,
            _musicVolumeDb + PausedMusicVolumeOffsetDb * progress
        );

        if (_musicPauseFilter != null)
            _musicPauseFilter.CutoffHz = Mathf.Lerp(
                NormalMusicCutoffHz,
                PausedMusicCutoffHz,
                progress
            );
    }

    private AudioStreamPlayer FindSfxPlayer()
    {
        int activeVoiceCount = 0;
        AudioStreamPlayer oldestPlayer = null;
        ulong oldestStartTime = ulong.MaxValue;

        foreach (var player in _sfxPlayers)
        {
            if (!player.Playing)
                continue;

            activeVoiceCount++;
            if (_sfxStartTimes.TryGetValue(player, out ulong startTime) && startTime < oldestStartTime)
            {
                oldestStartTime = startTime;
                oldestPlayer = player;
            }
        }

        if (activeVoiceCount < MaxConcurrentSfx)
        {
            foreach (var player in _sfxPlayers)
            {
                if (!player.Playing)
                    return player;
            }
        }

        // Reemplaza la voz más antigua para que los sonidos nuevos no se pierdan durante una ráfaga.
        return oldestPlayer;
    }

    private void UpdateSfxMix()
    {
        int activeVoiceCount = 0;
        foreach (var player in _sfxPlayers)
        {
            if (player.Playing)
                activeVoiceCount++;
        }

        if (activeVoiceCount == 0)
            return;

        // Compensa la suma de voces para mantener estable el nivel total y evitar clipping.
        float attenuationDb = -12f * Mathf.Log(activeVoiceCount) / Mathf.Log(10f);
        foreach (var player in _sfxPlayers)
        {
            if (player.Playing && _sfxBaseVolumes.TryGetValue(player, out float baseVolumeDb))
                player.VolumeDb = baseVolumeDb + attenuationDb;
        }
    }

    public void PlayMusic(string id)
    {
        if (!_musicLibrary.TryGetValue(id, out var audio))
        {
            GD.PushWarning($"AudioManager: No se encontró la música '{id}'.");
            return;
        }

        if (_activeMusicPlayer.Playing &&
            _activeMusicPlayer.Stream == audio.Stream)
        {
            return;
        }

        _musicCrossfadeTween?.Kill();

        AudioStreamPlayer newPlayer = _inactiveMusicPlayer;
        AudioStreamPlayer oldPlayer = _activeMusicPlayer;

        newPlayer.Stream = audio.Stream;
        newPlayer.PitchScale = audio.PitchScale;
        newPlayer.VolumeDb = -80.0f;
        newPlayer.Autoplay = false;
        newPlayer.Play();

        float newVolume = audio.VolumeDb;

        _activeMusicBaseVolumeDb = newVolume;

        _musicCrossfadeTween = CreateTween();
        _musicCrossfadeTween.SetPauseMode(Tween.TweenPauseMode.Process);

        if (oldPlayer.Playing)
        {
            _musicCrossfadeTween.Parallel().TweenProperty(
                oldPlayer,
                "volume_db",
                -80.0f,
                MusicCrossfadeDuration
            );
        }

        _musicCrossfadeTween.Parallel().TweenProperty(
            newPlayer,
            "volume_db",
            newVolume,
            MusicCrossfadeDuration
        );

        _musicCrossfadeTween.TweenCallback(
            Callable.From(() =>
            {
                oldPlayer.Stop();
                oldPlayer.Stream = null;
                oldPlayer.VolumeDb = -80.0f;

                _activeMusicPlayer = newPlayer;
                _inactiveMusicPlayer = oldPlayer;
            })
        );
    }

    public void StopMusic()
    {
        _musicCrossfadeTween?.Kill();

        _musicPlayerA.Stop();
        _musicPlayerB.Stop();

        _musicPlayerA.Stream = null;
        _musicPlayerB.Stream = null;

        _musicPlayerA.VolumeDb = -80.0f;
        _musicPlayerB.VolumeDb = -80.0f;

        _activeMusicPlayer = _musicPlayerA;
        _inactiveMusicPlayer = _musicPlayerB;
    }

    public void SetMasterVolume(float volumeDb)
    {
        SetBusVolume("Master", volumeDb);
    }

    public void SetMusicVolume(float volumeDb)
    {
        _musicVolumeDb = volumeDb;
        SetMusicPauseProgress(_musicPauseProgress);
    }

    public void SetSfxVolume(float volumeDb)
    {
        SetBusVolume("SFX", volumeDb);
    }

    private void SetBusVolume(string busName, float volumeDb)
    {
        int busIndex = AudioServer.GetBusIndex(busName);

        if (busIndex == -1)
        {
            GD.PushWarning($"AudioManager: No existe el bus '{busName}'.");
            return;
        }

        AudioServer.SetBusVolumeDb(busIndex, volumeDb);
    }

    public float GetMasterVolume()
    {
        return GetBusVolume("Master");
    }

    public float GetMusicVolume()
    {
        return _musicVolumeDb;
    }

    public float GetSfxVolume()
    {
        return GetBusVolume("SFX");
    }

    private float GetBusVolume(string busName)
    {
        int busIndex = AudioServer.GetBusIndex(busName);

        if (busIndex == -1)
        {
            GD.PushWarning($"AudioManager: No existe el bus '{busName}'.");
            return 0.0f;
        }

        return AudioServer.GetBusVolumeDb(busIndex);
    }
}