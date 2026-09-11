using Godot;
using System.Collections.Generic;

public partial class SettingsMenu : Control
{
    [Signal]
    public delegate void ClosedEventHandler();

    private const string SettingsPath = "user://settings.cfg";
    private const string AudioSection = "audio";
    private const string ControlsSection = "controls";
    private const float MinVolumeDb = -80.0f;
    private const float MaxVolumeDb = 0.0f;
    private const float DefaultVolumePercent = 100.0f;

    private static readonly string[] Actions =
    {
        "move_up",
        "move_down",
        "move_left",
        "move_right",
        "dash",
        "ability_q",
        "ability_e"
    };

    private readonly Dictionary<string, Button> _actionButtons = new();
    private Button _closeButton;
    private HSlider _masterSlider;
    private HSlider _musicSlider;
    private HSlider _sfxSlider;
    private Label _listeningLabel;
    private string _listeningAction;
    private ConfigFile _config;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;
        _config = new ConfigFile();
        LoadSettings();
        BuildControlRows();
        RefreshLocalizedText();

        _closeButton = GetNode<Button>("Panel/Margin/Content/CloseButton");
        _masterSlider.ValueChanged += value => SetVolume("master", value);
        _musicSlider.ValueChanged += value => SetVolume("music", value);
        _sfxSlider.ValueChanged += value => SetVolume("sfx", value);
        _closeButton.Pressed += Close;
    }

    public static void LoadSavedControls()
    {
        ConfigFile config = new();
        if (config.Load(SettingsPath) != Error.Ok)
            return;

        foreach (string action in Actions)
        {
            if (!config.HasSectionKey(ControlsSection, action))
                continue;

            if (TryDecodeKey((string)config.GetValue(ControlsSection, action), out InputEventKey keyEvent))
            {
                InputMap.ActionEraseEvents(action);
                InputMap.ActionAddEvent(action, keyEvent);
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (string.IsNullOrEmpty(_listeningAction))
            return;

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            RemapAction(_listeningAction, keyEvent);
            GetViewport().SetInputAsHandled();
        }
    }

    private void LoadSettings()
    {
        Error error = _config.Load(SettingsPath);
        ApplyAudioSetting("master", DbToPercent(AudioManager.Instance?.GetMasterVolume() ?? 0.0f));
        ApplyAudioSetting("music", DbToPercent(AudioManager.Instance?.GetMusicVolume() ?? 0.0f));
        ApplyAudioSetting("sfx", DbToPercent(AudioManager.Instance?.GetSfxVolume() ?? 0.0f));

        if (error == Error.Ok)
        {
            ApplyAudioSetting("master", GetSavedVolumePercent("master"));
            ApplyAudioSetting("music", GetSavedVolumePercent("music"));
            ApplyAudioSetting("sfx", GetSavedVolumePercent("sfx"));

            foreach (string action in Actions)
            {
                if (_config.HasSectionKey(ControlsSection, action))
                    ApplySavedAction(action, (string)_config.GetValue(ControlsSection, action));
            }
        }
    }

    private void ApplyAudioSetting(string bus, float volumePercent)
    {
        volumePercent = Mathf.Clamp(volumePercent, 0.0f, 100.0f);
        float volumeDb = PercentToDb(volumePercent);
        switch (bus)
        {
            case "master":
                _masterSlider ??= GetNode<HSlider>("Panel/Margin/Content/Audio/Sliders/Master");
                _masterSlider.Value = volumePercent;
                AudioManager.Instance?.SetMasterVolume(volumeDb);
                break;
            case "music":
                _musicSlider ??= GetNode<HSlider>("Panel/Margin/Content/Audio/Sliders/Music");
                _musicSlider.Value = volumePercent;
                AudioManager.Instance?.SetMusicVolume(volumeDb);
                break;
            case "sfx":
                _sfxSlider ??= GetNode<HSlider>("Panel/Margin/Content/Audio/Sliders/Sfx");
                _sfxSlider.Value = volumePercent;
                AudioManager.Instance?.SetSfxVolume(volumeDb);
                break;
        }
    }

    private float GetSavedVolumePercent(string bus)
    {
        if (_config.HasSectionKey(AudioSection, $"{bus}_percent"))
            return (float)_config.GetValue(AudioSection, $"{bus}_percent", DefaultVolumePercent);

        // Compatibility with settings saved by the previous dB-based sliders.
        float oldVolumeDb = (float)_config.GetValue(AudioSection, bus, MaxVolumeDb);
        return DbToPercent(oldVolumeDb);
    }

    private void SetVolume(string bus, double value)
    {
        float volumePercent = Mathf.Clamp((float)value, 0.0f, 100.0f);
        float volumeDb = PercentToDb(volumePercent);
        if (AudioManager.Instance != null)
        {
            switch (bus)
            {
                case "master":
                    AudioManager.Instance.SetMasterVolume(volumeDb);
                    break;
                case "music":
                    AudioManager.Instance.SetMusicVolume(volumeDb);
                    break;
                case "sfx":
                    AudioManager.Instance.SetSfxVolume(volumeDb);
                    break;
            }
        }

        _config.SetValue(AudioSection, $"{bus}_percent", volumePercent);
        _config.Save(SettingsPath);
    }

    private static float PercentToDb(float volumePercent)
    {
        if (volumePercent <= 0.0f)
            return MinVolumeDb;

        return Mathf.Clamp(20.0f * Mathf.Log(volumePercent / 100.0f) / Mathf.Log(10.0f), MinVolumeDb, MaxVolumeDb);
    }

    private static float DbToPercent(float volumeDb)
    {
        if (volumeDb <= MinVolumeDb)
            return 0.0f;

        return Mathf.Clamp(Mathf.Pow(10.0f, volumeDb / 20.0f) * 100.0f, 0.0f, 100.0f);
    }

    private void BuildControlRows()
    {
        VBoxContainer controls = GetNode<VBoxContainer>("Panel/Margin/Content/Controls/Rows");
        foreach (string action in Actions)
        {
            HBoxContainer row = new();
            row.AddThemeConstantOverride("separation", 18);

            Label label = new();
            label.Name = "Label";
            label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            label.Text = GetActionLabel(action);
            row.AddChild(label);

            Button button = new();
            button.CustomMinimumSize = new Vector2(130, 32);
            button.Text = GetActionText(action);
            button.Pressed += () => BeginListening(action);
            row.AddChild(button);

            controls.AddChild(row);
            _actionButtons[action] = button;
        }
    }

    private void ApplySavedAction(string action, string encodedEvent)
    {
        if (!TryDecodeKey(encodedEvent, out InputEventKey keyEvent))
            return;

        InputMap.ActionEraseEvents(action);
        InputMap.ActionAddEvent(action, keyEvent);
    }

    private void BeginListening(string action)
    {
        _listeningAction = action;
        _listeningLabel.Text = LocalizationManager.Translate("settings.press_key");
        foreach (var pair in _actionButtons)
            pair.Value.Disabled = true;
        _actionButtons[action].Disabled = false;
    }

    private void RemapAction(string action, InputEventKey keyEvent)
    {
        InputEventKey savedEvent = (InputEventKey)keyEvent.Duplicate();
        savedEvent.Unicode = 0;

        InputMap.ActionEraseEvents(action);
        InputMap.ActionAddEvent(action, savedEvent);
        _config.SetValue(ControlsSection, action, EncodeKey(savedEvent));
        _config.Save(SettingsPath);

        _actionButtons[action].Text = GetKeyText(savedEvent);
        _listeningAction = null;
        _listeningLabel.Text = LocalizationManager.Translate("settings.select_control");
        foreach (Button button in _actionButtons.Values)
            button.Disabled = false;
    }

    private string GetActionText(string action)
    {
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return events.Count > 0 && events[0] is InputEventKey keyEvent ? GetKeyText(keyEvent) : "-";
    }

    private string GetKeyText(InputEventKey keyEvent)
    {
        Key key = keyEvent.Keycode != Key.None ? keyEvent.Keycode : keyEvent.PhysicalKeycode;
        return OS.GetKeycodeString((Key)key);
    }

    private static string EncodeKey(InputEventKey keyEvent)
    {
        return $"{(int)keyEvent.Keycode},{(int)keyEvent.PhysicalKeycode}";
    }

    private static bool TryDecodeKey(string encodedEvent, out InputEventKey keyEvent)
    {
        keyEvent = null;
        string[] values = encodedEvent.Split(',');
        if (values.Length == 2 && int.TryParse(values[0], out int keycode) && int.TryParse(values[1], out int physicalKeycode))
        {
            keyEvent = new InputEventKey
            {
                Keycode = (Key)keycode,
                PhysicalKeycode = (Key)physicalKeycode
            };
            return true;
        }

        // Compatibility with the old format, which stored only the physical key.
        if (int.TryParse(encodedEvent, out int oldPhysicalKeycode))
        {
            keyEvent = new InputEventKey
            {
                PhysicalKeycode = (Key)oldPhysicalKeycode,
                Keycode = (Key)oldPhysicalKeycode
            };
            return true;
        }

        return false;
    }

    private string GetActionLabel(string action)
    {
        return LocalizationManager.Translate($"settings.control.{action}");
    }

    private void RefreshLocalizedText()
    {
        GetNode<Label>("Panel/Margin/Content/Title").Text = LocalizationManager.Translate("settings.title");
        GetNode<Label>("Panel/Margin/Content/Audio/Title").Text = LocalizationManager.Translate("settings.audio");
        GetNode<Label>("Panel/Margin/Content/Audio/Sliders/MasterLabel").Text = LocalizationManager.Translate("settings.master");
        GetNode<Label>("Panel/Margin/Content/Audio/Sliders/MusicLabel").Text = LocalizationManager.Translate("settings.music");
        GetNode<Label>("Panel/Margin/Content/Audio/Sliders/SfxLabel").Text = LocalizationManager.Translate("settings.sfx");
        GetNode<Label>("Panel/Margin/Content/Controls/Title").Text = LocalizationManager.Translate("settings.controls");
        _listeningLabel = GetNode<Label>("Panel/Margin/Content/Controls/ListeningLabel");
        _listeningLabel.Text = LocalizationManager.Translate("settings.select_control");
        _closeButton = GetNode<Button>("Panel/Margin/Content/CloseButton");
        _closeButton.Text = LocalizationManager.Translate("common.close");
    }

    private void Close()
    {
        EmitSignal(SignalName.Closed);
        QueueFree();
    }
}
