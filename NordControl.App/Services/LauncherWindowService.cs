using Avalonia.Controls.ApplicationLifetimes;
using NordControl.App.ViewModels.Launcher;
using NordControl.App.Views.Launcher;
using NordControl.Core.Services;

namespace NordControl.App.Services;

public sealed class LauncherWindowService
{
    private readonly ILauncherService launcherService;
    private LauncherWindow? window;

    public LauncherWindowService(ILauncherService launcherService)
    {
        this.launcherService = launcherService;
    }

    public void ShowLauncher()
    {
        if (window is { IsVisible: true })
        {
            window.Activate();
            return;
        }

        window = new LauncherWindow();
        window.DataContext = new LauncherViewModel(launcherService, HideLauncher);
        window.Closed += (_, _) => window = null;
        window.Show();
        window.Activate();
    }

    public void ToggleLauncher()
    {
        if (window is { IsVisible: true })
        {
            HideLauncher();
            return;
        }

        ShowLauncher();
    }

    public void HideLauncher()
    {
        window?.Close();
        window = null;
    }
}
