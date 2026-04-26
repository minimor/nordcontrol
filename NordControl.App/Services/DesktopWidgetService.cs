using Avalonia;
using Avalonia.Controls;
using NordControl.App.ViewModels.Widgets;
using NordControl.App.Views.Widgets;
using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Core.Services;

namespace NordControl.App.Services;

public sealed class DesktopWidgetService : IDesktopWidgetService
{
    private readonly IAppStateService appStateService;
    private readonly Dictionary<string, DesktopWidgetWindow> windows = [];

    public DesktopWidgetService(IAppStateService appStateService)
    {
        this.appStateService = appStateService;
    }

    public IReadOnlyList<DesktopWidgetDefinition> GetDefinitions()
    {
        return DesktopWidgetCatalog.Definitions;
    }

    public DesktopWidgetSettings LoadSettings(AppSettings settings)
    {
        settings.Normalize();
        return settings.Customization.DesktopWidgets;
    }

    public WidgetOperationResult SaveSettings(DesktopWidgetSettings settings)
    {
        settings.Normalize();
        appStateService.Settings.Customization.DesktopWidgets = settings;
        appStateService.Save();
        return WidgetOperationResult.Succeeded("Desktop widget settings saved.");
    }

    public WidgetOperationResult ShowWidgets()
    {
        var settings = appStateService.Settings.Customization.DesktopWidgets;
        settings.Normalize();

        if (!settings.EnableWidgets)
        {
            return WidgetOperationResult.Failed("Desktop widgets are disabled. Enable widgets before showing overlay windows.");
        }

        var enabledWidgets = settings.Widgets
            .Where(widget => widget.IsEnabled && DesktopWidgetCatalog.IsImplementedWidgetType(widget.WidgetType))
            .ToList();

        if (enabledWidgets.Count == 0)
        {
            return WidgetOperationResult.Failed("No implemented desktop widgets are enabled.");
        }

        var shown = 0;
        foreach (var widget in enabledWidgets)
        {
            if (windows.ContainsKey(widget.Id))
            {
                continue;
            }

            try
            {
                var definition = DesktopWidgetCatalog.GetDefinitionOrDefault(widget.WidgetType);
                var viewModel = new DesktopWidgetWindowViewModel(widget, definition, settings.ShowWidgetBackground, settings.GlobalOpacity);
                var window = new DesktopWidgetWindow
                {
                    DataContext = viewModel,
                    Width = widget.Width,
                    Height = widget.Height,
                    Topmost = widget.IsAlwaysOnTop,
                    Opacity = Math.Clamp(settings.GlobalOpacity, 0.2, 1.0),
                    ShowInTaskbar = false,
                    CanResize = !settings.LockWidgetPositions,
                    Position = new PixelPoint((int)Math.Round(widget.X), (int)Math.Round(widget.Y))
                };

                window.Closed += (_, _) =>
                {
                    if (window.DataContext is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    windows.Remove(widget.Id);
                };

                window.Show();
                windows[widget.Id] = window;
                shown++;
            }
            catch (Exception ex)
            {
                return WidgetOperationResult.Failed(
                    "Could not create a desktop widget window.",
                    ex.Message);
            }
        }

        return WidgetOperationResult.Succeeded(
            shown == 0 ? "Desktop widgets are already visible." : $"Shown {shown} desktop widget window(s).",
            "Overlay windows are app-owned and can be hidden from NordControl.");
    }

    public WidgetOperationResult HideWidgets()
    {
        var openWindows = windows.Values.ToList();
        foreach (var window in openWindows)
        {
            window.Close();
        }

        windows.Clear();
        return WidgetOperationResult.Succeeded("Desktop widget windows hidden.");
    }

    public WidgetOperationResult ToggleWidgets()
    {
        return windows.Count > 0 ? HideWidgets() : ShowWidgets();
    }

    public WidgetOperationResult ResetWidgetLayout()
    {
        var settings = appStateService.Settings.Customization.DesktopWidgets;
        settings.Widgets = DesktopWidgetCatalog.CreateDefaultWidgetInstances();
        settings.Normalize();
        appStateService.Save();

        if (windows.Count > 0)
        {
            HideWidgets();
            ShowWidgets();
        }

        return WidgetOperationResult.Succeeded("Desktop widget layout reset.");
    }

    public WidgetOperationResult ShowStartupWidgetsIfEnabled()
    {
        var settings = appStateService.Settings.Customization.DesktopWidgets;
        settings.Normalize();

        if (!settings.EnableWidgets || !settings.StartWidgetsWithApp)
        {
            return WidgetOperationResult.Succeeded("Desktop widgets are not configured to start with the app.");
        }

        return ShowWidgets();
    }
}
