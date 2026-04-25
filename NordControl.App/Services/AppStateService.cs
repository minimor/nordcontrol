using NordControl.Core.Models;
using NordControl.Core.Services;

namespace NordControl.App.Services;

public sealed class AppStateService : IAppStateService
{
    private readonly IAppSettingsService appSettingsService;
    private AppSettings settings;

    public AppStateService(IAppSettingsService appSettingsService)
    {
        this.appSettingsService = appSettingsService;
        settings = this.appSettingsService.Load();
        settings.Normalize();
        LastStatusMessage = this.appSettingsService.LastStatusMessage;
    }

    public event EventHandler? SettingsChanged;

    public AppSettings Settings => settings;

    public string SettingsFilePath => appSettingsService.SettingsFilePath;

    public string LastStatusMessage { get; private set; }

    public void Reload()
    {
        settings = appSettingsService.Load();
        settings.Normalize();
        LastStatusMessage = appSettingsService.LastStatusMessage;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Save()
    {
        settings.Normalize();
        appSettingsService.Save(settings);
        LastStatusMessage = appSettingsService.LastStatusMessage;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetToDefaults()
    {
        settings = appSettingsService.ResetToDefaults();
        settings.Normalize();
        LastStatusMessage = appSettingsService.LastStatusMessage;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
