using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NordControl.App.ViewModels;
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
        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
