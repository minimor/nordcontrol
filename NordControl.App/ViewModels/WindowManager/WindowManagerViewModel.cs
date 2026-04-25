using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NordControl.App.Services;
using NordControl.Core.Models;
using NordControl.Core.Services;
using NordControl.Core.Windows;

namespace NordControl.App.ViewModels.WindowManager;

public partial class WindowManagerViewModel : ViewModelBase, IDisposable
{
    private readonly IWindowManagerService windowManagerService;
    private readonly IAppStateService appStateService;
    private IReadOnlyList<WindowInfo> allWindows = [];
    private DispatcherTimer? autoRefreshTimer;
    private bool isRefreshingWindows;

    public WindowManagerViewModel()
        : this(
            new DesignTimeWindowManagerService(),
            new AppStateService(new DesignTimeAppSettingsService()))
    {
    }

    public WindowManagerViewModel(
        IWindowManagerService windowManagerService,
        IAppStateService appStateService)
    {
        this.windowManagerService = windowManagerService;
        this.appStateService = appStateService;
        this.appStateService.SettingsChanged += OnSettingsChanged;

        if (CurrentSettings.RefreshOnStartup)
        {
            _ = RefreshWindowsAsync();
        }
        else
        {
            WindowOperationMessage = CurrentSettings.AutoRefreshIntervalSeconds > 0
                ? "Window list not loaded on startup. Auto-refresh will run after the configured interval."
                : "Window list not loaded. Click Refresh.";
            ApplyWindowFilter();
        }

        StartOrUpdateAutoRefreshTimer();
    }

    public ObservableCollection<WindowRowViewModel> FilteredWindows { get; } = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string windowOperationMessage = "Window list not loaded. Click Refresh.";

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

    private WindowManagerSettings CurrentSettings => appStateService.Settings.WindowManager;

    public bool HasFilteredWindows => FilteredWindowCount > 0;

    public bool ShowNoWindowsEmptyState => OpenWindowCount == 0;

    public bool ShowNoSearchResultsEmptyState => OpenWindowCount > 0 && FilteredWindowCount == 0;

    public string LastRefreshText => LastWindowRefreshAt?.ToString("HH:mm:ss") ?? "Not refreshed";

    public string CurrentProfile => "Normal";

    partial void OnSearchTextChanged(string value)
    {
        ApplyWindowFilter();
    }

    [RelayCommand]
    public async Task RefreshWindowsAsync()
    {
        await LoadWindowsAsync();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    public void Dispose()
    {
        StopAutoRefreshTimer();
        appStateService.SettingsChanged -= OnSettingsChanged;
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
        allWindows = windows;
        LastWindowRefreshAt = DateTime.Now;
        WindowOperationMessage = operationMessage;
        UpdateWindowCounts();
        ApplyWindowFilter();
    }

    private void ApplyWindowLoadFailure(Exception ex)
    {
        allWindows = [];
        LastWindowRefreshAt = DateTime.Now;
        WindowOperationMessage = $"Could not load windows: {ex.Message}";
        UpdateWindowCounts();
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
        var filtered = WindowInfoFilter.Apply(GetVisibleWindows(), SearchText);

        FilteredWindows.Clear();
        foreach (var window in filtered)
        {
            FilteredWindows.Add(new WindowRowViewModel(window, PinWindow, UnpinWindow));
        }

        FilteredWindowCount = FilteredWindows.Count;
    }

    private IReadOnlyList<WindowInfo> GetVisibleWindows()
    {
        if (CurrentSettings.ShowUnknownProcesses)
        {
            return allWindows;
        }

        return allWindows
            .Where(window => !string.Equals(window.ProcessName, "Unknown", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void UpdateWindowCounts()
    {
        var visibleWindows = GetVisibleWindows();
        OpenWindowCount = visibleWindows.Count;
        TopMostWindowCount = visibleWindows.Count(window => window.IsTopMost);
    }

    private void StartOrUpdateAutoRefreshTimer()
    {
        StopAutoRefreshTimer();

        var intervalSeconds = CurrentSettings.AutoRefreshIntervalSeconds;
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

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        StartOrUpdateAutoRefreshTimer();
        UpdateWindowCounts();
        ApplyWindowFilter();
    }
}
