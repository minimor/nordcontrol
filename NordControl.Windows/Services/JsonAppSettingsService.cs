using System.Text.Json;
using NordControl.Core.Models;
using NordControl.Core.Services;

namespace NordControl.Windows.Services;

public sealed class JsonAppSettingsService : IAppSettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public JsonAppSettingsService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NordControl",
            "settings.json"))
    {
    }

    public JsonAppSettingsService(string settingsFilePath)
    {
        SettingsFilePath = settingsFilePath;
    }

    public string SettingsFilePath { get; }

    public string LastStatusMessage { get; private set; } = "Settings ready.";

    public AppSettings Load()
    {
        EnsureSettingsDirectory();

        if (!File.Exists(SettingsFilePath))
        {
            var defaults = AppSettings.CreateDefault();
            Save(defaults);
            LastStatusMessage = "Created default settings.";
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions)
                ?? AppSettings.CreateDefault();

            settings.Normalize();
            LastStatusMessage = "Settings loaded.";
            return settings;
        }
        catch (JsonException)
        {
            PreserveBrokenSettingsFile();

            var defaults = AppSettings.CreateDefault();
            Save(defaults);
            LastStatusMessage = "Settings file was invalid and has been reset to defaults.";
            return defaults;
        }
        catch (IOException ex)
        {
            LastStatusMessage = $"Could not read settings: {ex.Message}";
            return AppSettings.CreateDefault();
        }
        catch (UnauthorizedAccessException ex)
        {
            LastStatusMessage = $"Could not access settings: {ex.Message}";
            return AppSettings.CreateDefault();
        }
    }

    public void Save(AppSettings settings)
    {
        EnsureSettingsDirectory();

        settings.Normalize();
        var json = JsonSerializer.Serialize(settings, SerializerOptions);
        File.WriteAllText(SettingsFilePath, json);
        LastStatusMessage = "Settings saved.";
    }

    public AppSettings ResetToDefaults()
    {
        var defaults = AppSettings.CreateDefault();
        Save(defaults);
        LastStatusMessage = "Settings reset to defaults.";
        return defaults;
    }

    private void EnsureSettingsDirectory()
    {
        var directory = Path.GetDirectoryName(SettingsFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private void PreserveBrokenSettingsFile()
    {
        if (!File.Exists(SettingsFilePath))
        {
            return;
        }

        var directory = Path.GetDirectoryName(SettingsFilePath) ?? string.Empty;
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var brokenPath = Path.Combine(directory, $"settings.broken-{timestamp}.json");

        try
        {
            File.Move(SettingsFilePath, brokenPath, overwrite: false);
        }
        catch (IOException)
        {
            var fallbackPath = Path.Combine(directory, $"settings.broken-{timestamp}-{Guid.NewGuid():N}.json");
            File.Move(SettingsFilePath, fallbackPath, overwrite: false);
        }
    }
}
