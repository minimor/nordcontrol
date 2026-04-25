using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface IAppSettingsService
{
    string SettingsFilePath { get; }

    string LastStatusMessage { get; }

    AppSettings Load();

    void Save(AppSettings settings);

    AppSettings ResetToDefaults();
}
