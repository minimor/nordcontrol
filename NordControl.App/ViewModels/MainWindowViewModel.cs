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
    private IReadOnlyList<WindowInfo> allWindows = [];
    private AppSettings currentSettings = AppSettings.CreateDefault();
    private DispatcherTimer? autoRefreshTimer;
    private bool isApplyingSettings;
    private bool isRefreshingWindows;

    public MainWindowViewModel()
        : this(new DesignTimeAppSettingsService(), new DesignTimePlatformInfoService(), new DesignTimeWindowManagerService())
    {
    }

    public MainWindowViewModel(
        IAppSettingsService appSettingsService,
        IPlatformInfoService platformInfoService,
        IWindowManagerService windowManagerService)
    {
        this.appSettingsService = appSettingsService;
        this.platformInfoService = platformInfoService;
        this.windowManagerService = windowManagerService;

        Modules = AppModuleCatalog.DefaultModules
            .Select(module => new ShellModuleViewModel(module))
            .ToList();

        PlatformInfo = this.platformInfoService.GetPlatformInfo();
        SettingsFilePath = this.appSettingsService.SettingsFilePath;
        LoadSettingsIntoEditor(selectSavedModule: true);

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

    public string SettingsFilePath { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveModuleName))]
    [NotifyPropertyChangedFor(nameof(ActiveModuleDescription))]
    [NotifyPropertyChangedFor(nameof(ActiveModuleStatus))]
    [NotifyPropertyChangedFor(nameof(IsDashboardSelected))]
    [NotifyPropertyChangedFor(nameof(IsWindowManagerSelected))]
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

    public bool IsSettingsSelected => SelectedModule?.Key == "settings";

    public bool IsPlaceholderModuleSelected =>
        !IsDashboardSelected && !IsWindowManagerSelected && !IsSettingsSelected;

    public bool HasFilteredWindows => FilteredWindowCount > 0;

    public bool ShowNoWindowsEmptyState => OpenWindowCount == 0;

    public bool ShowNoSearchResultsEmptyState => OpenWindowCount > 0 && FilteredWindowCount == 0;

    public string LastRefreshText => LastWindowRefreshAt?.ToString("HH:mm:ss") ?? "Not refreshed";

    public string CurrentProfile => "Normal";

    public string AutoRefreshStatus => AutoRefreshIntervalSeconds <= 0
        ? "Auto-refresh disabled."
        : $"Auto-refresh every {AutoRefreshIntervalSeconds} seconds.";

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
        currentSettings.Normalize();
        AutoRefreshIntervalSeconds = currentSettings.WindowManager.AutoRefreshIntervalSeconds;
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
}
