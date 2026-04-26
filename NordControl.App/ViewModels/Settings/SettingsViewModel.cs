using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NordControl.App.Services;
using NordControl.App.ViewModels.Customization;

namespace NordControl.App.ViewModels.Settings;

public partial class SettingsViewModel : ViewModelBase, IDisposable
{
    private readonly IAppStateService appStateService;
    private readonly CustomizationViewModel customizationViewModel;
    private bool isApplyingSettings;

    public SettingsViewModel()
        : this(
            new AppStateService(new DesignTimeAppSettingsService()),
            new CustomizationViewModel())
    {
    }

    public SettingsViewModel(
        IAppStateService appStateService,
        CustomizationViewModel customizationViewModel)
    {
        this.appStateService = appStateService;
        this.customizationViewModel = customizationViewModel;
        SettingsFilePath = this.appStateService.SettingsFilePath;
        this.appStateService.SettingsChanged += OnSettingsChanged;
        ApplySettingsToEditor();
    }

    public string SettingsFilePath { get; }

    [ObservableProperty]
    private string settingsStatusMessage = "Settings ready.";

    [ObservableProperty]
    private string selectedTheme = "Dark";

    [ObservableProperty]
    private bool refreshOnStartup = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AutoRefreshStatus))]
    private int autoRefreshIntervalSeconds;

    [ObservableProperty]
    private bool confirmBeforePinning;

    [ObservableProperty]
    private bool showUnknownProcesses = true;

    [ObservableProperty]
    private string selectedCustomizationPresetKey = "fluent-dark";

    [ObservableProperty]
    private string selectedThemePackageKey = "fluent-dark";

    [ObservableProperty]
    private string lastExportedThemePath = string.Empty;

    [ObservableProperty]
    private string lastImportedThemePath = string.Empty;

    [ObservableProperty]
    private bool applyThemeToNordControlShell = true;

    [ObservableProperty]
    private string currentCustomizationSectionName = "Overview";

    [ObservableProperty]
    private string nordControlAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool enableGlassStyleInApp = true;

    [ObservableProperty]
    private bool allowLowRiskWindowsPersonalization = true;

    [ObservableProperty]
    private bool enableLauncher = true;

    [ObservableProperty]
    private bool launcherStartWithApp = true;

    [ObservableProperty]
    private string launcherHotkeyGesture = "Ctrl+Space";

    [ObservableProperty]
    private bool launcherShowOnStartup;

    [ObservableProperty]
    private bool launcherIncludeNordControlCommands = true;

    [ObservableProperty]
    private bool launcherIncludeApps = true;

    [ObservableProperty]
    private bool launcherIncludeSystemActions = true;

    [ObservableProperty]
    private bool launcherCloseAfterAction = true;

    [ObservableProperty]
    private int launcherMaxResults = 10;

    public string AutoRefreshStatus => AutoRefreshIntervalSeconds <= 0
        ? "Auto-refresh disabled."
        : $"Auto-refresh every {AutoRefreshIntervalSeconds} seconds.";

    [RelayCommand]
    private void SaveSettings()
    {
        WriteEditorToSettings();
        appStateService.Save();
        SettingsStatusMessage = appStateService.LastStatusMessage;
    }

    [RelayCommand]
    private void ReloadSettings()
    {
        appStateService.Reload();
        SettingsStatusMessage = appStateService.LastStatusMessage;
    }

    [RelayCommand]
    private void ResetSettings()
    {
        appStateService.ResetToDefaults();
        SettingsStatusMessage = appStateService.LastStatusMessage;
    }

    public void Dispose()
    {
        appStateService.SettingsChanged -= OnSettingsChanged;
    }

    private void ApplySettingsToEditor()
    {
        isApplyingSettings = true;

        var settings = appStateService.Settings;
        SelectedTheme = settings.Theme;
        RefreshOnStartup = settings.WindowManager.RefreshOnStartup;
        AutoRefreshIntervalSeconds = settings.WindowManager.AutoRefreshIntervalSeconds;
        ConfirmBeforePinning = settings.WindowManager.ConfirmBeforePinning;
        ShowUnknownProcesses = settings.WindowManager.ShowUnknownProcesses;
        SelectedCustomizationPresetKey = settings.Customization.SelectedPresetKey;
        SelectedThemePackageKey = settings.Customization.SelectedThemePackageKey;
        LastExportedThemePath = string.IsNullOrWhiteSpace(settings.Customization.LastExportedThemePath)
            ? "No theme exported yet."
            : settings.Customization.LastExportedThemePath;
        LastImportedThemePath = string.IsNullOrWhiteSpace(settings.Customization.LastImportedThemePath)
            ? "No theme imported yet."
            : settings.Customization.LastImportedThemePath;
        ApplyThemeToNordControlShell = settings.Customization.ApplyThemeToNordControlShell;
        CurrentCustomizationSectionName = customizationViewModel.CurrentCustomizationSectionName;
        NordControlAccentColorHex = settings.Customization.NordControlAccentColorHex;
        EnableGlassStyleInApp = settings.Customization.EnableGlassStyleInApp;
        AllowLowRiskWindowsPersonalization = settings.Customization.AllowLowRiskWindowsPersonalization;
        EnableLauncher = settings.Launcher.EnableLauncher;
        LauncherStartWithApp = settings.Launcher.StartWithApp;
        LauncherHotkeyGesture = settings.Launcher.HotkeyGesture;
        LauncherShowOnStartup = settings.Launcher.ShowOnStartup;
        LauncherIncludeNordControlCommands = settings.Launcher.IncludeNordControlCommands;
        LauncherIncludeApps = settings.Launcher.IncludeApps;
        LauncherIncludeSystemActions = settings.Launcher.IncludeSystemActions;
        LauncherCloseAfterAction = settings.Launcher.CloseAfterAction;
        LauncherMaxResults = settings.Launcher.MaxResults;
        SettingsStatusMessage = appStateService.LastStatusMessage;

        isApplyingSettings = false;
    }

    private void WriteEditorToSettings()
    {
        var settings = appStateService.Settings;
        settings.Theme = string.IsNullOrWhiteSpace(SelectedTheme) ? "Dark" : SelectedTheme;
        settings.WindowManager.RefreshOnStartup = RefreshOnStartup;
        settings.WindowManager.AutoRefreshIntervalSeconds = Math.Max(0, AutoRefreshIntervalSeconds);
        settings.WindowManager.ConfirmBeforePinning = ConfirmBeforePinning;
        settings.WindowManager.ShowUnknownProcesses = ShowUnknownProcesses;
        settings.Customization.SelectedPresetKey = SelectedCustomizationPresetKey;
        settings.Customization.SelectedThemePackageKey = SelectedThemePackageKey;
        settings.Customization.ApplyThemeToNordControlShell = ApplyThemeToNordControlShell;
        settings.Customization.NordControlAccentColorHex = NordControlAccentColorHex;
        settings.Customization.EnableGlassStyleInApp = EnableGlassStyleInApp;
        settings.Customization.AllowLowRiskWindowsPersonalization = AllowLowRiskWindowsPersonalization;
        settings.Launcher.EnableLauncher = EnableLauncher;
        settings.Launcher.StartWithApp = LauncherStartWithApp;
        settings.Launcher.HotkeyGesture = LauncherHotkeyGesture;
        settings.Launcher.ShowOnStartup = LauncherShowOnStartup;
        settings.Launcher.IncludeNordControlCommands = LauncherIncludeNordControlCommands;
        settings.Launcher.IncludeApps = LauncherIncludeApps;
        settings.Launcher.IncludeSystemActions = LauncherIncludeSystemActions;
        settings.Launcher.CloseAfterAction = LauncherCloseAfterAction;
        settings.Launcher.MaxResults = LauncherMaxResults;
        settings.Normalize();
        AutoRefreshIntervalSeconds = settings.WindowManager.AutoRefreshIntervalSeconds;
        NordControlAccentColorHex = settings.Customization.NordControlAccentColorHex;
        LauncherHotkeyGesture = settings.Launcher.HotkeyGesture;
        LauncherMaxResults = settings.Launcher.MaxResults;
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        if (!isApplyingSettings)
        {
            ApplySettingsToEditor();
        }
    }
}
