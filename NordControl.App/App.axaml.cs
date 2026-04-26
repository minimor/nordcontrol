using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NordControl.App.Services;
using NordControl.App.ViewModels;
using NordControl.App.ViewModels.Customization;
using NordControl.App.ViewModels.Dashboard;
using NordControl.App.ViewModels.Settings;
using NordControl.App.ViewModels.Shell;
using NordControl.App.ViewModels.WindowManager;
using NordControl.App.Views;
using NordControl.Core.Services;
using NordControl.Windows.Services;

namespace NordControl.App;

public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            serviceProvider = ConfigureServices();

            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>(),
            };

            var desktopWidgetService = serviceProvider.GetRequiredService<DesktopWidgetService>();
            desktopWidgetService.ShowStartupWidgetsIfEnabled();
            desktop.Exit += (_, _) => desktopWidgetService.HideWidgets();

            if (serviceProvider.GetRequiredService<IAppStateService>().Settings.Launcher.ShowOnStartup)
            {
                serviceProvider.GetRequiredService<LauncherWindowService>().ShowLauncher();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAppSettingsService, JsonAppSettingsService>();
        services.AddSingleton<IPlatformInfoService, WindowsPlatformInfoService>();
        services.AddSingleton<IWindowManagerService, WindowsWindowManagerService>();
        services.AddSingleton<IWindowsPersonalizationService, WindowsPersonalizationService>();
        services.AddSingleton<ITaskbarService, WindowsTaskbarService>();
        services.AddSingleton<DesktopWidgetService>();
        services.AddSingleton<IDesktopWidgetService>(provider => provider.GetRequiredService<DesktopWidgetService>());
        services.AddSingleton<IThemePackageService, JsonThemePackageService>();
        services.AddSingleton<IAppStateService, AppStateService>();
        services.AddSingleton<LauncherService>();
        services.AddSingleton<ILauncherService>(provider => provider.GetRequiredService<LauncherService>());
        services.AddSingleton<LauncherWindowService>();
        services.AddSingleton<WindowManagerViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<CustomizationViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
