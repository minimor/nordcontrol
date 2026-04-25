using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Core.Services;
using NordControl.Core.Windows;

namespace NordControl.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IAppSettingsService appSettingsService;
    private readonly IPlatformInfoService platformInfoService;
    private readonly IWindowManagerService windowManagerService;
    private readonly IWindowsPersonalizationService windowsPersonalizationService;
    private IReadOnlyList<WindowInfo> allWindows = [];
    private AppSettings currentSettings = AppSettings.CreateDefault();
    private DispatcherTimer? autoRefreshTimer;
    private bool isApplyingSettings;
    private bool isRefreshingWindows;

    public MainWindowViewModel()
        : this(
            new DesignTimeAppSettingsService(),
            new DesignTimePlatformInfoService(),
            new DesignTimeWindowManagerService(),
            new DesignTimeWindowsPersonalizationService())
    {
    }

    public MainWindowViewModel(
        IAppSettingsService appSettingsService,
        IPlatformInfoService platformInfoService,
        IWindowManagerService windowManagerService,
        IWindowsPersonalizationService windowsPersonalizationService)
    {
        this.appSettingsService = appSettingsService;
        this.platformInfoService = platformInfoService;
        this.windowManagerService = windowManagerService;
        this.windowsPersonalizationService = windowsPersonalizationService;

        Modules = AppModuleCatalog.DefaultModules
            .Select(module => new ShellModuleViewModel(module))
            .ToList();
        CustomizationPresets = CustomizationPresetCatalog.DefaultPresets
            .Select(preset => new CustomizationPresetViewModel(preset, ApplyCustomizationPreset))
            .ToList();
        CustomizationSections = CustomizationSectionCatalog.Sections
            .Select(section => new CustomizationSectionViewModel(section, SelectCustomizationSection))
            .ToList();

        PlatformInfo = this.platformInfoService.GetPlatformInfo();
        SettingsFilePath = this.appSettingsService.SettingsFilePath;
        LoadSettingsIntoEditor(selectSavedModule: true);
        LoadPersonalizationState();

        if (currentSettings.WindowManager.RefreshOnStartup)
        {
            _ = RefreshWindowsAsync();
        }
        else
        {
            WindowOperationMessage = currentSettings.WindowManager.AutoRefreshIntervalSeconds > 0
                ? "Window list not loaded on startup. Auto-refresh will run after the configured interval."
                : "Window list not loaded. Click Refresh.";
            ApplyWindowFilter();
        }

        StartOrUpdateAutoRefreshTimer();
    }

    public IReadOnlyList<ShellModuleViewModel> Modules { get; }

    public PlatformInfo PlatformInfo { get; }

    public ObservableCollection<WindowRowViewModel> FilteredWindows { get; } = [];

    public IReadOnlyList<CustomizationPresetViewModel> CustomizationPresets { get; }

    public IReadOnlyList<CustomizationSectionViewModel> CustomizationSections { get; }

    public ObservableCollection<CustomizationFeatureCardViewModel> CurrentCustomizationFeatureCards { get; } = [];

    public string SettingsFilePath { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveModuleName))]
    [NotifyPropertyChangedFor(nameof(ActiveModuleDescription))]
    [NotifyPropertyChangedFor(nameof(ActiveModuleStatus))]
    [NotifyPropertyChangedFor(nameof(IsDashboardSelected))]
    [NotifyPropertyChangedFor(nameof(IsWindowManagerSelected))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationSelected))]
    [NotifyPropertyChangedFor(nameof(IsSettingsSelected))]
    [NotifyPropertyChangedFor(nameof(IsPlaceholderModuleSelected))]
    private ShellModuleViewModel? selectedModule;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string windowOperationMessage = "Window list not loaded. Click Refresh.";

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
    private string personalizationStatusMessage = "Customization Studio ready.";

    [ObservableProperty]
    private string appsTheme = "Unknown";

    [ObservableProperty]
    private string systemTheme = "Unknown";

    [ObservableProperty]
    private bool transparencyEffectsEnabled;

    [ObservableProperty]
    private string windowsAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool accentColorOnTitleBars;

    [ObservableProperty]
    private string wallpaperPath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PersonalizationLastLoadedText))]
    private DateTime? personalizationLastLoadedAt;

    [ObservableProperty]
    private string selectedCustomizationPresetKey = "fluent-dark";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentCustomizationSectionName))]
    [NotifyPropertyChangedFor(nameof(CurrentCustomizationSectionDescription))]
    [NotifyPropertyChangedFor(nameof(CurrentCustomizationSectionBadge))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationOverviewSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationThemesSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationPlanningSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationRiskLabSection))]
    private CustomizationSectionViewModel? selectedCustomizationSection;

    [ObservableProperty]
    private string nordControlAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool enableGlassStyleInApp = true;

    [ObservableProperty]
    private bool allowLowRiskWindowsPersonalization = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastRefreshText))]
    private DateTime? lastWindowRefreshAt;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFilteredWindows))]
    [NotifyPropertyChangedFor(nameof(ShowNoWindowsEmptyState))]
    [NotifyPropertyChangedFor(nameof(ShowNoSearchResultsEmptyState))]
    private int filteredWindowCount;

    [ObservableProperty]
    private int openWindowCount;

    [ObservableProperty]
    private int topMostWindowCount;

    public string ActiveModuleName => SelectedModule?.DisplayName ?? "NordControl";

    public string ActiveModuleDescription =>
        SelectedModule?.Description ?? "Select a module from the sidebar.";

    public string ActiveModuleStatus => SelectedModule?.Status ?? "Ready";

    public string PlatformSummary =>
        $"{PlatformInfo.OperatingSystem} | {PlatformInfo.Runtime} | {PlatformInfo.Architecture}";

    public bool IsDashboardSelected => SelectedModule?.Key == "dashboard";

    public bool IsWindowManagerSelected => SelectedModule?.Key == "window-manager";

    public bool IsCustomizationSelected => SelectedModule?.Key == "customization";

    public bool IsSettingsSelected => SelectedModule?.Key == "settings";

    public bool IsPlaceholderModuleSelected =>
        !IsDashboardSelected && !IsWindowManagerSelected && !IsCustomizationSelected && !IsSettingsSelected;

    public bool HasFilteredWindows => FilteredWindowCount > 0;

    public bool ShowNoWindowsEmptyState => OpenWindowCount == 0;

    public bool ShowNoSearchResultsEmptyState => OpenWindowCount > 0 && FilteredWindowCount == 0;

    public string LastRefreshText => LastWindowRefreshAt?.ToString("HH:mm:ss") ?? "Not refreshed";

    public string CurrentProfile => "Normal";

    public string AutoRefreshStatus => AutoRefreshIntervalSeconds <= 0
        ? "Auto-refresh disabled."
        : $"Auto-refresh every {AutoRefreshIntervalSeconds} seconds.";

    public string PersonalizationLastLoadedText =>
        PersonalizationLastLoadedAt?.ToString("HH:mm:ss") ?? "Not loaded";

    public string CurrentCustomizationSectionName => SelectedCustomizationSection?.Name ?? "Overview";

    public string CurrentCustomizationSectionDescription =>
        SelectedCustomizationSection?.Description ?? "Snapshot, safety status, and the desktop environment roadmap.";

    public string CurrentCustomizationSectionBadge => SelectedCustomizationSection?.Badge ?? "Safe Layer";

    public bool IsCustomizationOverviewSection => SelectedCustomizationSection?.Key == "overview";

    public bool IsCustomizationThemesSection => SelectedCustomizationSection?.Key == "themes";

    public bool IsCustomizationRiskLabSection => SelectedCustomizationSection?.IsRiskLab == true;

    public bool IsCustomizationPlanningSection =>
        !IsCustomizationOverviewSection && !IsCustomizationThemesSection;

    partial void OnSelectedModuleChanged(ShellModuleViewModel? value)
    {
        if (!isApplyingSettings && value is not null)
        {
            currentSettings.LastSelectedModuleKey = value.Key;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyWindowFilter();
    }

    partial void OnSelectedCustomizationSectionChanged(CustomizationSectionViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        currentSettings.Customization.LastSelectedSectionKey = value.Key;
        if (!isApplyingSettings)
        {
            appSettingsService.Save(currentSettings);
        }

        RefreshCustomizationFeatureCards();
    }

    [RelayCommand]
    private async Task RefreshWindowsAsync()
    {
        await LoadWindowsAsync();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private void RefreshPersonalization()
    {
        LoadPersonalizationState();
    }

    [RelayCommand]
    private void ToggleAppsTheme()
    {
        if (!CanApplyLowRiskWindowsPersonalization())
        {
            return;
        }

        var nextTheme = AppsTheme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
        ApplyPersonalizationOperation(windowsPersonalizationService.SetAppsTheme(nextTheme));
    }

    [RelayCommand]
    private void ToggleSystemTheme()
    {
        if (!CanApplyLowRiskWindowsPersonalization())
        {
            return;
        }

        var nextTheme = SystemTheme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
        ApplyPersonalizationOperation(windowsPersonalizationService.SetSystemTheme(nextTheme));
    }

    [RelayCommand]
    private void ToggleTransparencyEffects()
    {
        if (!CanApplyLowRiskWindowsPersonalization())
        {
            return;
        }

        ApplyPersonalizationOperation(windowsPersonalizationService.SetTransparencyEffects(!TransparencyEffectsEnabled));
    }

    [RelayCommand]
    private void ToggleAccentColorOnTitleBars()
    {
        if (!CanApplyLowRiskWindowsPersonalization())
        {
            return;
        }

        ApplyPersonalizationOperation(windowsPersonalizationService.SetAccentColorOnTitleBars(!AccentColorOnTitleBars));
    }

    [RelayCommand]
    private void SaveSettings()
    {
        WriteEditorToSettings();
        appSettingsService.Save(currentSettings);
        SettingsStatusMessage = appSettingsService.LastStatusMessage;
        StartOrUpdateAutoRefreshTimer();
        ApplyWindowFilter();
    }

    [RelayCommand]
    private void ReloadSettings()
    {
        LoadSettingsIntoEditor(selectSavedModule: true);
        StartOrUpdateAutoRefreshTimer();
        ApplyWindowFilter();
    }

    [RelayCommand]
    private void ResetSettings()
    {
        currentSettings = appSettingsService.ResetToDefaults();
        ApplySettingsToEditor(selectSavedModule: true);
        SettingsStatusMessage = appSettingsService.LastStatusMessage;
        StartOrUpdateAutoRefreshTimer();
        ApplyWindowFilter();
    }

    public void Dispose()
    {
        StopAutoRefreshTimer();
    }

    private async Task LoadWindowsAsync(string? operationMessage = null, bool isAutoRefresh = false)
    {
        if (isRefreshingWindows)
        {
            return;
        }

        isRefreshingWindows = true;

        try
        {
            var windows = await Task.Run(windowManagerService.GetOpenWindows);
            ApplyLoadedWindows(
                windows,
                operationMessage ?? (isAutoRefresh
                    ? $"Auto-refreshed {windows.Count} open windows."
                    : $"Loaded {windows.Count} open windows."));
        }
        catch (Exception ex)
        {
            ApplyWindowLoadFailure(ex);
        }
        finally
        {
            isRefreshingWindows = false;
        }
    }

    private void ApplyLoadedWindows(IReadOnlyList<WindowInfo> windows, string operationMessage)
    {
        if (!currentSettings.WindowManager.ShowUnknownProcesses)
        {
            windows = windows
                .Where(window => !string.Equals(window.ProcessName, "Unknown", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        allWindows = windows;
        OpenWindowCount = allWindows.Count;
        TopMostWindowCount = allWindows.Count(window => window.IsTopMost);
        LastWindowRefreshAt = DateTime.Now;
        WindowOperationMessage = operationMessage;
        ApplyWindowFilter();
    }

    private void ApplyWindowLoadFailure(Exception ex)
    {
        allWindows = [];
        OpenWindowCount = 0;
        TopMostWindowCount = 0;
        LastWindowRefreshAt = DateTime.Now;
        WindowOperationMessage = $"Could not load windows: {ex.Message}";
        ApplyWindowFilter();
    }

    private void PinWindow(WindowRowViewModel row)
    {
        _ = ApplyWindowOperationAsync(windowManagerService.SetTopMost(row.Handle));
    }

    private void UnpinWindow(WindowRowViewModel row)
    {
        _ = ApplyWindowOperationAsync(windowManagerService.RemoveTopMost(row.Handle));
    }

    private async Task ApplyWindowOperationAsync(WindowOperationResult result)
    {
        await LoadWindowsAsync(result.Message);
        if (!result.Success && result.NativeErrorCode is { } errorCode)
        {
            WindowOperationMessage = $"{result.Message} Native error: {errorCode}.";
        }
    }

    private void ApplyWindowFilter()
    {
        var filtered = WindowInfoFilter.Apply(allWindows, SearchText);

        FilteredWindows.Clear();
        foreach (var window in filtered)
        {
            FilteredWindows.Add(new WindowRowViewModel(window, PinWindow, UnpinWindow));
        }

        FilteredWindowCount = FilteredWindows.Count;
    }

    private void LoadSettingsIntoEditor(bool selectSavedModule)
    {
        currentSettings = appSettingsService.Load();
        ApplySettingsToEditor(selectSavedModule);
        SettingsStatusMessage = appSettingsService.LastStatusMessage;
    }

    private void ApplySettingsToEditor(bool selectSavedModule)
    {
        currentSettings.Normalize();
        isApplyingSettings = true;

        SelectedTheme = currentSettings.Theme;
        RefreshOnStartup = currentSettings.WindowManager.RefreshOnStartup;
        AutoRefreshIntervalSeconds = currentSettings.WindowManager.AutoRefreshIntervalSeconds;
        ConfirmBeforePinning = currentSettings.WindowManager.ConfirmBeforePinning;
        ShowUnknownProcesses = currentSettings.WindowManager.ShowUnknownProcesses;
        SelectedCustomizationPresetKey = currentSettings.Customization.SelectedPresetKey;
        SelectedCustomizationSection = CustomizationSections
            .FirstOrDefault(section => section.Key == currentSettings.Customization.LastSelectedSectionKey)
            ?? CustomizationSections.FirstOrDefault(section => section.Key == CustomizationSectionCatalog.DefaultSectionKey)
            ?? CustomizationSections.FirstOrDefault();
        NordControlAccentColorHex = currentSettings.Customization.NordControlAccentColorHex;
        EnableGlassStyleInApp = currentSettings.Customization.EnableGlassStyleInApp;
        AllowLowRiskWindowsPersonalization = currentSettings.Customization.AllowLowRiskWindowsPersonalization;

        if (selectSavedModule)
        {
            SelectedModule = Modules.FirstOrDefault(module => module.Key == currentSettings.LastSelectedModuleKey)
                ?? Modules.FirstOrDefault();
        }

        isApplyingSettings = false;
    }

    private void WriteEditorToSettings()
    {
        currentSettings.Theme = string.IsNullOrWhiteSpace(SelectedTheme) ? "Dark" : SelectedTheme;
        currentSettings.LastSelectedModuleKey = SelectedModule?.Key ?? "dashboard";
        currentSettings.WindowManager.RefreshOnStartup = RefreshOnStartup;
        currentSettings.WindowManager.AutoRefreshIntervalSeconds = Math.Max(0, AutoRefreshIntervalSeconds);
        currentSettings.WindowManager.ConfirmBeforePinning = ConfirmBeforePinning;
        currentSettings.WindowManager.ShowUnknownProcesses = ShowUnknownProcesses;
        currentSettings.Customization.SelectedPresetKey = SelectedCustomizationPresetKey;
        currentSettings.Customization.LastSelectedSectionKey =
            SelectedCustomizationSection?.Key ?? CustomizationSectionCatalog.DefaultSectionKey;
        currentSettings.Customization.NordControlAccentColorHex = NordControlAccentColorHex;
        currentSettings.Customization.EnableGlassStyleInApp = EnableGlassStyleInApp;
        currentSettings.Customization.AllowLowRiskWindowsPersonalization = AllowLowRiskWindowsPersonalization;
        currentSettings.Normalize();
        AutoRefreshIntervalSeconds = currentSettings.WindowManager.AutoRefreshIntervalSeconds;
        SelectedCustomizationPresetKey = currentSettings.Customization.SelectedPresetKey;
        SelectedCustomizationSection = CustomizationSections
            .FirstOrDefault(section => section.Key == currentSettings.Customization.LastSelectedSectionKey)
            ?? SelectedCustomizationSection;
        NordControlAccentColorHex = currentSettings.Customization.NordControlAccentColorHex;
    }

    private void LoadPersonalizationState()
    {
        try
        {
            var state = windowsPersonalizationService.GetCurrentState();
            AppsTheme = state.AppsTheme;
            SystemTheme = state.SystemTheme;
            TransparencyEffectsEnabled = state.TransparencyEffectsEnabled;
            WindowsAccentColorHex = state.AccentColorHex;
            AccentColorOnTitleBars = state.AccentColorOnTitleBars;
            WallpaperPath = string.IsNullOrWhiteSpace(state.WallpaperPath) ? "Not available" : state.WallpaperPath;
            PersonalizationLastLoadedAt = state.LastLoadedAt;
            PersonalizationStatusMessage = "Windows style snapshot refreshed.";
        }
        catch (Exception ex)
        {
            PersonalizationStatusMessage = $"Could not load Windows style: {ex.Message}";
        }
    }

    private void ApplyPersonalizationOperation(PersonalizationOperationResult result)
    {
        PersonalizationStatusMessage = result.Success
            ? result.Message
            : $"{result.Message}{(string.IsNullOrWhiteSpace(result.Requires) ? string.Empty : $" Requires: {result.Requires}.")}";

        LoadPersonalizationState();
        if (!result.Success)
        {
            PersonalizationStatusMessage = $"{result.Message}{(string.IsNullOrWhiteSpace(result.Requires) ? string.Empty : $" Requires: {result.Requires}.")}";
        }
    }

    private bool CanApplyLowRiskWindowsPersonalization()
    {
        if (AllowLowRiskWindowsPersonalization)
        {
            return true;
        }

        PersonalizationStatusMessage = "Low-risk Windows personalization is disabled in settings.";
        return false;
    }

    private void ApplyCustomizationPreset(CustomizationPresetViewModel preset)
    {
        SelectedCustomizationPresetKey = preset.Key;
        NordControlAccentColorHex = preset.AccentColorHex;
        EnableGlassStyleInApp = true;
        currentSettings.Customization.SelectedPresetKey = preset.Key;
        currentSettings.Customization.NordControlAccentColorHex = preset.AccentColorHex;
        currentSettings.Customization.EnableGlassStyleInApp = true;
        appSettingsService.Save(currentSettings);
        PersonalizationStatusMessage = $"{preset.Name} applied to NordControl preview and saved.";
    }

    private void SelectCustomizationSection(CustomizationSectionViewModel section)
    {
        SelectedCustomizationSection = section;
    }

    private void RefreshCustomizationFeatureCards()
    {
        CurrentCustomizationFeatureCards.Clear();

        var sectionKey = SelectedCustomizationSection?.Key ?? CustomizationSectionCatalog.DefaultSectionKey;
        foreach (var card in CustomizationSectionCatalog.GetFeatureCards(sectionKey))
        {
            CurrentCustomizationFeatureCards.Add(new CustomizationFeatureCardViewModel(card));
        }
    }

    private void StartOrUpdateAutoRefreshTimer()
    {
        StopAutoRefreshTimer();

        var intervalSeconds = currentSettings.WindowManager.AutoRefreshIntervalSeconds;
        if (intervalSeconds <= 0)
        {
            return;
        }

        autoRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(intervalSeconds)
        };

        autoRefreshTimer.Tick += OnAutoRefreshTimerTick;
        autoRefreshTimer.Start();
    }

    private void StopAutoRefreshTimer()
    {
        if (autoRefreshTimer is null)
        {
            return;
        }

        autoRefreshTimer.Stop();
        autoRefreshTimer.Tick -= OnAutoRefreshTimerTick;
        autoRefreshTimer = null;
    }

    private async void OnAutoRefreshTimerTick(object? sender, EventArgs e)
    {
        await LoadWindowsAsync(isAutoRefresh: true);
    }

    private sealed class DesignTimeAppSettingsService : IAppSettingsService
    {
        public string SettingsFilePath { get; } = @"%AppData%\NordControl\settings.json";

        public string LastStatusMessage { get; private set; } = "Design-time settings loaded.";

        public AppSettings Load()
        {
            LastStatusMessage = "Design-time settings loaded.";
            return AppSettings.CreateDefault();
        }

        public void Save(AppSettings settings)
        {
            LastStatusMessage = "Design-time settings saved.";
        }

        public AppSettings ResetToDefaults()
        {
            LastStatusMessage = "Design-time settings reset.";
            return AppSettings.CreateDefault();
        }
    }

    private sealed class DesignTimePlatformInfoService : IPlatformInfoService
    {
        public PlatformInfo GetPlatformInfo()
        {
            return new PlatformInfo("Windows", ".NET", "x64", true);
        }
    }

    private sealed class DesignTimeWindowManagerService : IWindowManagerService
    {
        public IReadOnlyList<WindowInfo> GetOpenWindows()
        {
            return
            [
                new WindowInfo(0x10001, "Untitled - Notepad", "notepad", 1200, false, DateTime.Now),
                new WindowInfo(0x10002, "NordControl", "NordControl.App", 2400, true, DateTime.Now)
            ];
        }

        public WindowOperationResult SetTopMost(nint hwnd)
        {
            return WindowOperationResult.Succeeded("Window pinned as topmost.", hwnd);
        }

        public WindowOperationResult RemoveTopMost(nint hwnd)
        {
            return WindowOperationResult.Succeeded("Window returned to normal stacking.", hwnd);
        }

        public bool IsTopMost(nint hwnd)
        {
            return hwnd == 0x10002;
        }
    }

    private sealed class DesignTimeWindowsPersonalizationService : IWindowsPersonalizationService
    {
        public WindowsPersonalizationState GetCurrentState()
        {
            return new WindowsPersonalizationState(
                "Dark",
                "Dark",
                true,
                "#4CC2FF",
                false,
                @"C:\Windows\Web\Wallpaper\Windows\img0.jpg",
                DateTime.Now);
        }

        public PersonalizationOperationResult SetAppsTheme(string theme)
        {
            return PersonalizationOperationResult.Succeeded("Design-time apps theme updated.");
        }

        public PersonalizationOperationResult SetSystemTheme(string theme)
        {
            return PersonalizationOperationResult.Succeeded("Design-time system theme updated.");
        }

        public PersonalizationOperationResult SetTransparencyEffects(bool enabled)
        {
            return PersonalizationOperationResult.Succeeded("Design-time transparency updated.");
        }

        public PersonalizationOperationResult SetAccentColor(string hexColor)
        {
            return PersonalizationOperationResult.Failed("Design-time accent color is preview-only.", "App-only");
        }

        public PersonalizationOperationResult SetAccentColorOnTitleBars(bool enabled)
        {
            return PersonalizationOperationResult.Succeeded("Design-time title bar accent updated.");
        }

        public PersonalizationOperationResult SetWallpaper(string filePath)
        {
            return PersonalizationOperationResult.Failed("Design-time wallpaper is preview-only.", "App-only");
        }
    }
}
