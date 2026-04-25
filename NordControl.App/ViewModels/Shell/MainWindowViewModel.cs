using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NordControl.App.Services;
using NordControl.App.ViewModels.Customization;
using NordControl.App.ViewModels.Dashboard;
using NordControl.App.ViewModels.Settings;
using NordControl.App.ViewModels.WindowManager;
using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Core.Services;

namespace NordControl.App.ViewModels.Shell;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IAppStateService appStateService;
    private readonly WindowManagerViewModel windowManagerViewModel;
    private readonly DashboardViewModel dashboardViewModel;
    private readonly SettingsViewModel settingsViewModel;
    private readonly CustomizationViewModel customizationViewModel;
    private readonly Dictionary<string, ViewModelBase> pageViewModels;
    private bool isApplyingSettings;

    public MainWindowViewModel()
        : this(
            new AppStateService(new DesignTimeAppSettingsService()),
            new DesignTimePlatformInfoService(),
            new WindowManagerViewModel(),
            new CustomizationViewModel())
    {
    }

    public MainWindowViewModel(
        IAppStateService appStateService,
        IPlatformInfoService platformInfoService,
        WindowManagerViewModel windowManagerViewModel,
        CustomizationViewModel customizationViewModel)
        : this(
            appStateService,
            platformInfoService,
            windowManagerViewModel,
            new DashboardViewModel(windowManagerViewModel),
            new SettingsViewModel(appStateService, customizationViewModel),
            customizationViewModel)
    {
    }

    public MainWindowViewModel(
        IAppStateService appStateService,
        IPlatformInfoService platformInfoService,
        WindowManagerViewModel windowManagerViewModel,
        DashboardViewModel dashboardViewModel,
        SettingsViewModel settingsViewModel,
        CustomizationViewModel customizationViewModel)
    {
        this.appStateService = appStateService;
        this.windowManagerViewModel = windowManagerViewModel;
        this.dashboardViewModel = dashboardViewModel;
        this.settingsViewModel = settingsViewModel;
        this.customizationViewModel = customizationViewModel;

        Modules = AppModuleCatalog.DefaultModules
            .Select(module => new ShellModuleViewModel(module))
            .ToList();
        PlatformInfo = platformInfoService.GetPlatformInfo();

        pageViewModels = new Dictionary<string, ViewModelBase>
        {
            ["dashboard"] = this.dashboardViewModel,
            ["window-manager"] = this.windowManagerViewModel,
            ["customization"] = this.customizationViewModel,
            ["settings"] = this.settingsViewModel
        };

        isApplyingSettings = true;
        SelectedModule = Modules.FirstOrDefault(module => module.Key == this.appStateService.Settings.LastSelectedModuleKey)
            ?? Modules.FirstOrDefault();
        isApplyingSettings = false;

        UpdateCurrentPageViewModel();
    }

    public IReadOnlyList<ShellModuleViewModel> Modules { get; }

    public PlatformInfo PlatformInfo { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveModuleName))]
    [NotifyPropertyChangedFor(nameof(ActiveModuleDescription))]
    [NotifyPropertyChangedFor(nameof(ActiveModuleStatus))]
    private ShellModuleViewModel? selectedModule;

    [ObservableProperty]
    private ViewModelBase? currentPageViewModel;

    public string ActiveModuleName => SelectedModule?.DisplayName ?? "NordControl";

    public string ActiveModuleDescription =>
        SelectedModule?.Description ?? "Select a module from the sidebar.";

    public string ActiveModuleStatus => SelectedModule?.Status ?? "Ready";

    public string PlatformSummary =>
        $"{PlatformInfo.OperatingSystem} | {PlatformInfo.Runtime} | {PlatformInfo.Architecture}";

    partial void OnSelectedModuleChanged(ShellModuleViewModel? value)
    {
        UpdateCurrentPageViewModel();

        if (isApplyingSettings || value is null)
        {
            return;
        }

        appStateService.Settings.LastSelectedModuleKey = value.Key;
        appStateService.Save();
    }

    public void Dispose()
    {
        dashboardViewModel.Dispose();
        windowManagerViewModel.Dispose();
        settingsViewModel.Dispose();
        customizationViewModel.Dispose();
    }

    private void UpdateCurrentPageViewModel()
    {
        if (SelectedModule is null)
        {
            CurrentPageViewModel = null;
            return;
        }

        CurrentPageViewModel = pageViewModels.TryGetValue(SelectedModule.Key, out var pageViewModel)
            ? pageViewModel
            : new ModulePlaceholderViewModel(SelectedModule);
    }
}
