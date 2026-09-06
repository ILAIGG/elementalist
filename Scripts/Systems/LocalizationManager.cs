using Godot;
using System;
using System.Globalization;

public partial class LocalizationManager : Node
{
    private const string SettingsPath = "user://settings.cfg";
    private const string SettingsSection = "localization";
    private const string SettingsKey = "locale";

    public static LocalizationManager Instance { get; private set; }
    public const string DefaultLocale = "en";
    public const string SpanishLocale = "es";

    [Signal]
    public delegate void LanguageChangedEventHandler();

    public string CurrentLocale { get; private set; } = DefaultLocale;

    public override void _Ready()
    {
        Instance = this;
        SetLocale(LoadLocale(), false);
    }

    public void SetLocale(string locale, bool save = true)
    {
        string normalizedLocale = NormalizeLocale(locale);
        if (CurrentLocale == normalizedLocale && TranslationServer.GetLocale() == normalizedLocale)
            return;

        CurrentLocale = normalizedLocale;
        TranslationServer.SetLocale(CurrentLocale);

        if (save)
            SaveLocale(CurrentLocale);

        EmitSignal(SignalName.LanguageChanged);
    }

    public static string Translate(string key, params object[] arguments)
    {
        string translated = TranslationServer.Translate(key);
        if (arguments.Length == 0)
            return translated;

        return string.Format(CultureInfo.InvariantCulture, translated, arguments);
    }

    private static string LoadLocale()
    {
        ConfigFile settings = new();
        if (settings.Load(SettingsPath) != Error.Ok)
            return DefaultLocale;

        Variant value = settings.GetValue(SettingsSection, SettingsKey, DefaultLocale);
        return value.VariantType == Variant.Type.String ? value.AsString() : DefaultLocale;
    }

    private static void SaveLocale(string locale)
    {
        ConfigFile settings = new();
        settings.Load(SettingsPath);
        settings.SetValue(SettingsSection, SettingsKey, locale);
        settings.Save(SettingsPath);
    }

    private static string NormalizeLocale(string locale)
    {
        return locale == SpanishLocale ? SpanishLocale : DefaultLocale;
    }
}
