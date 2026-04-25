using NordControl.Core.Models;

namespace NordControl.App.Services;

public interface IAppStateService
{
    event EventHandler? SettingsChanged;

    AppSettings Settings { get; }

    string SettingsFilePath { get; }

    string LastStatusMessage { get; }

    void Reload();

    void Save();

    void ResetToDefaults();
}
