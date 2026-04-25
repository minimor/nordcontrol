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
    private string currentCustomizationSectionName = "Overview";

    [ObservableProperty]
    private string nordControlAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool enableGlassStyleInApp = true;

    [ObservableProperty]
    private bool allowLowRiskWindowsPersonalization = true;

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
        CurrentCustomizationSectionName = customizationViewModel.CurrentCustomizationSectionName;
        NordControlAccentColorHex = settings.Customization.NordControlAccentColorHex;
        EnableGlassStyleInApp = settings.Customization.EnableGlassStyleInApp;
        AllowLowRiskWindowsPersonalization = settings.Customization.AllowLowRiskWindowsPersonalization;
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
        settings.Customization.NordControlAccentColorHex = NordControlAccentColorHex;
        settings.Customization.EnableGlassStyleInApp = EnableGlassStyleInApp;
        settings.Customization.AllowLowRiskWindowsPersonalization = AllowLowRiskWindowsPersonalization;
        settings.Normalize();
        AutoRefreshIntervalSeconds = settings.WindowManager.AutoRefreshIntervalSeconds;
        NordControlAccentColorHex = settings.Customization.NordControlAccentColorHex;
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        if (!isApplyingSettings)
        {
            ApplySettingsToEditor();
        }
    }
}
