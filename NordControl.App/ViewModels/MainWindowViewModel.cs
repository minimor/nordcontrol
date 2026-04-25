using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Core.Services;
using NordControl.Core.Windows;

namespace NordControl.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IPlatformInfoService platformInfoService;
    private readonly IWindowManagerService windowManagerService;
    private IReadOnlyList<WindowInfo> allWindows = [];

    public MainWindowViewModel()
        : this(new DesignTimePlatformInfoService(), new DesignTimeWindowManagerService())
    {
    }

    public MainWindowViewModel(
        IPlatformInfoService platformInfoService,
        IWindowManagerService windowManagerService)
    {
        this.platformInfoService = platformInfoService;
        this.windowManagerService = windowManagerService;

        Modules = AppModuleCatalog.DefaultModules
            .Select(module => new ShellModuleViewModel(module))
            .ToList();

        PlatformInfo = this.platformInfoService.GetPlatformInfo();
        SelectedModule = Modules.FirstOrDefault();
        RefreshWindows();
    }

    public IReadOnlyList<ShellModuleViewModel> Modules { get; }

    public PlatformInfo PlatformInfo { get; }

    public ObservableCollection<WindowRowViewModel> FilteredWindows { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveModuleName))]
    [NotifyPropertyChangedFor(nameof(ActiveModuleDescription))]
    [NotifyPropertyChangedFor(nameof(ActiveModuleStatus))]
    [NotifyPropertyChangedFor(nameof(IsDashboardSelected))]
    [NotifyPropertyChangedFor(nameof(IsWindowManagerSelected))]
    [NotifyPropertyChangedFor(nameof(IsPlaceholderModuleSelected))]
    private ShellModuleViewModel? selectedModule;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string windowOperationMessage = "Window list loaded.";

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

    public bool IsPlaceholderModuleSelected => !IsDashboardSelected && !IsWindowManagerSelected;

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
    private void RefreshWindows()
    {
        LoadWindows();
    }

    private void LoadWindows(string? operationMessage = null)
    {
        try
        {
            allWindows = windowManagerService.GetOpenWindows();
            OpenWindowCount = allWindows.Count;
            TopMostWindowCount = allWindows.Count(window => window.IsTopMost);
            LastWindowRefreshAt = DateTime.Now;
            WindowOperationMessage = operationMessage ?? $"Loaded {OpenWindowCount} open windows.";
            ApplyWindowFilter();
        }
        catch (Exception ex)
        {
            allWindows = [];
            OpenWindowCount = 0;
            TopMostWindowCount = 0;
            LastWindowRefreshAt = DateTime.Now;
            WindowOperationMessage = $"Could not load windows: {ex.Message}";
            ApplyWindowFilter();
        }
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private void PinWindow(WindowRowViewModel row)
    {
        ApplyWindowOperation(windowManagerService.SetTopMost(row.Handle));
    }

    private void UnpinWindow(WindowRowViewModel row)
    {
        ApplyWindowOperation(windowManagerService.RemoveTopMost(row.Handle));
    }

    private void ApplyWindowOperation(WindowOperationResult result)
    {
        LoadWindows(result.Message);
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
