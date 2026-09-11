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
    private const float DefaultMasterVolumePercent = 50.0f;
    private const float DefaultMusicVolumePercent = 100.0f;
    private const float DefaultSfxVolumePercent = 100.0f;

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
    private SpinBox _masterValueInput;
    private SpinBox _musicValueInput;
    private SpinBox _sfxValueInput;
    private Label _listeningLabel;
    private string _listeningAction;
    private ConfigFile _config;
    private Button _resetButton;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;
        _config = new ConfigFile();
        CacheAudioControls();
        LoadSettings();
        BuildControlRows();
        RefreshLocalizedText();

        _closeButton = GetNode<Button>("Panel/Margin/Content/CloseButton");
        _masterSlider.ValueChanged += value => SetVolume("master", value);
        _musicSlider.ValueChanged += value => SetVolume("music", value);
        _sfxSlider.ValueChanged += value => SetVolume("sfx", value);
        _masterValueInput.ValueChanged += value => SetVolume("master", value);
        _musicValueInput.ValueChanged += value => SetVolume("music", value);
        _sfxValueInput.ValueChanged += value => SetVolume("sfx", value);
        _resetButton = GetNode<Button>("Panel/Margin/Content/ResetButton");
        _resetButton.Pressed += ResetDefaults;
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
        if (error != Error.Ok)
        {
            ApplyAudioSetting("master", DefaultMasterVolumePercent);
            ApplyAudioSetting("music", DefaultMusicVolumePercent);
            ApplyAudioSetting("sfx", DefaultSfxVolumePercent);
            return;
        }

        if (error == Error.Ok)
        {
            ApplyAudioSetting("master", GetSavedVolumePercent("master", DefaultMasterVolumePercent));
            ApplyAudioSetting("music", GetSavedVolumePercent("music", DefaultMusicVolumePercent));
            ApplyAudioSetting("sfx", GetSavedVolumePercent("sfx", DefaultSfxVolumePercent));

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
                _masterSlider.Value = volumePercent;
                _masterValueInput.Value = volumePercent;
                AudioManager.Instance?.SetMasterVolume(volumeDb);
                break;
            case "music":
                _musicSlider.Value = volumePercent;
                _musicValueInput.Value = volumePercent;
                AudioManager.Instance?.SetMusicVolume(volumeDb);
                break;
            case "sfx":
                _sfxSlider.Value = volumePercent;
                _sfxValueInput.Value = volumePercent;
                AudioManager.Instance?.SetSfxVolume(volumeDb);
                break;
        }
    }

    private void CacheAudioControls()
    {
        _masterSlider = GetNode<HSlider>("Panel/Margin/Content/Audio/Sliders/MasterRow/Master");
        _musicSlider = GetNode<HSlider>("Panel/Margin/Content/Audio/Sliders/MusicRow/Music");
        _sfxSlider = GetNode<HSlider>("Panel/Margin/Content/Audio/Sliders/SfxRow/Sfx");
        _masterValueInput = GetNode<SpinBox>("Panel/Margin/Content/Audio/Sliders/MasterRow/MasterValue");
        _musicValueInput = GetNode<SpinBox>("Panel/Margin/Content/Audio/Sliders/MusicRow/MusicValue");
        _sfxValueInput = GetNode<SpinBox>("Panel/Margin/Content/Audio/Sliders/SfxRow/SfxValue");
    }

    private float GetSavedVolumePercent(string bus, float defaultPercent)
    {
        if (_config.HasSectionKey(AudioSection, $"{bus}_percent"))
            return (float)_config.GetValue(AudioSection, $"{bus}_percent", defaultPercent);

        // Compatibility with settings saved by the previous dB-based sliders.
        float oldVolumeDb = (float)_config.GetValue(AudioSection, bus, PercentToDb(defaultPercent));
        return DbToPercent(oldVolumeDb);
    }

    private void SetVolume(string bus, double value)
    {
        float volumePercent = Mathf.Clamp((float)value, 0.0f, 100.0f);
        UpdateVolumeControls(bus, volumePercent);
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

    private void UpdateVolumeControls(string bus, float volumePercent)
    {
        SpinBox input = bus switch
        {
            "master" => _masterValueInput,
            "music" => _musicValueInput,
            "sfx" => _sfxValueInput,
            _ => null
        };
        input?.SetValueNoSignal(volumePercent);

        HSlider slider = bus switch
        {
            "master" => _masterSlider,
            "music" => _musicSlider,
            "sfx" => _sfxSlider,
            _ => null
        };
        slider?.SetValueNoSignal(volumePercent);
    }

    private void ResetDefaults()
    {
        InputMap.LoadFromProjectSettings();
        _config.EraseSection(AudioSection);
        _config.EraseSection(ControlsSection);
        _config.Save(SettingsPath);

        ApplyAudioSetting("master", DefaultMasterVolumePercent);
        ApplyAudioSetting("music", DefaultMusicVolumePercent);
        ApplyAudioSetting("sfx", DefaultSfxVolumePercent);

        foreach (string action in Actions)
            _actionButtons[action].Text = GetActionText(action);
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
        GetNode<Label>("Panel/Margin/Content/Audio/Sliders/MasterRow/MasterLabel").Text = LocalizationManager.Translate("settings.master");
        GetNode<Label>("Panel/Margin/Content/Audio/Sliders/MusicRow/MusicLabel").Text = LocalizationManager.Translate("settings.music");
        GetNode<Label>("Panel/Margin/Content/Audio/Sliders/SfxRow/SfxLabel").Text = LocalizationManager.Translate("settings.sfx");
        GetNode<Label>("Panel/Margin/Content/Controls/Title").Text = LocalizationManager.Translate("settings.controls");
        _listeningLabel = GetNode<Label>("Panel/Margin/Content/Controls/ListeningLabel");
        _listeningLabel.Text = LocalizationManager.Translate("settings.select_control");
        _closeButton = GetNode<Button>("Panel/Margin/Content/CloseButton");
        _closeButton.Text = LocalizationManager.Translate("common.close");
        _resetButton = GetNode<Button>("Panel/Margin/Content/ResetButton");
        _resetButton.Text = LocalizationManager.Translate("settings.reset_defaults");
    }

    private void Close()
    {
        EmitSignal(SignalName.Closed);
        QueueFree();
    }
}
