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

    public bool IsWidgetsVisible => windows.Count > 0;

    public int ActiveWidgetCount => windows.Count;

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
        SynchronizeOpenWindows(settings);
        return WidgetOperationResult.Succeeded("Desktop widget settings saved.");
    }

    public WidgetOperationResult ShowWidgets()
    {
        var settings = appStateService.Settings.Customization.DesktopWidgets;
        NormalizeSettingsForDisplay(settings);

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
                var window = CreateWidgetWindow(widget, settings);
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
        NormalizeSettingsForDisplay(settings);
        appStateService.Save();

        if (windows.Count > 0)
        {
            HideWidgets();
            ShowWidgets();
        }

        return WidgetOperationResult.Succeeded("Desktop widget layout reset.");
    }

    public WidgetOperationResult SaveWidgetBounds(string widgetId, double x, double y, double width, double height)
    {
        var settings = appStateService.Settings.Customization.DesktopWidgets;
        var widget = settings.Widgets.FirstOrDefault(item => string.Equals(item.Id, widgetId, StringComparison.Ordinal));
        if (widget is null)
        {
            return WidgetOperationResult.Failed("Could not save widget bounds.", "Widget instance was not found.");
        }

        widget.X = SnapToEdge(x);
        widget.Y = SnapToEdge(y);
        widget.Width = width;
        widget.Height = height;
        NormalizeSettingsForDisplay(settings);
        appStateService.Save();

        return WidgetOperationResult.Succeeded("Widget layout saved.");
    }

    public WidgetOperationResult HideWidget(string widgetId, bool disable)
    {
        if (windows.Remove(widgetId, out var window))
        {
            window.Close();
        }

        if (disable)
        {
            var settings = appStateService.Settings.Customization.DesktopWidgets;
            var widget = settings.Widgets.FirstOrDefault(item => string.Equals(item.Id, widgetId, StringComparison.Ordinal));
            if (widget is not null)
            {
                widget.IsEnabled = false;
                appStateService.Save();
            }
        }

        return WidgetOperationResult.Succeeded(disable ? "Widget disabled and hidden." : "Widget hidden.");
    }

    public string GetWidgetLayoutSummary()
    {
        var settings = appStateService.Settings.Customization.DesktopWidgets;
        settings.Normalize();

        return string.Join(
            " | ",
            settings.Widgets
                .Where(widget => DesktopWidgetCatalog.IsImplementedWidgetType(widget.WidgetType))
                .Select(widget =>
                {
                    var definition = DesktopWidgetCatalog.GetDefinitionOrDefault(widget.WidgetType);
                    return $"{definition.Name}: {(widget.IsEnabled ? "on" : "off")} @ {Math.Round(widget.X)},{Math.Round(widget.Y)} {Math.Round(widget.Width)}x{Math.Round(widget.Height)}";
                }));
    }

    public WidgetOperationResult ShowStartupWidgetsIfEnabled()
    {
        var settings = appStateService.Settings.Customization.DesktopWidgets;
        NormalizeSettingsForDisplay(settings);

        if (!settings.EnableWidgets || !settings.StartWidgetsWithApp)
        {
            return WidgetOperationResult.Succeeded("Desktop widgets are not configured to start with the app.");
        }

        return ShowWidgets();
    }

    private DesktopWidgetWindow CreateWidgetWindow(
        DesktopWidgetInstanceSettings widget,
        DesktopWidgetSettings settings)
    {
        var definition = DesktopWidgetCatalog.GetDefinitionOrDefault(widget.WidgetType);
        var viewModel = new DesktopWidgetWindowViewModel(
            widget,
            definition,
            settings.ShowWidgetBackground,
            settings.GlobalOpacity,
            settings.LockWidgetPositions,
            SaveWidgetBounds,
            id => HideWidget(id, disable: true));

                var window = new DesktopWidgetWindow
                {
                    DataContext = viewModel,
                    Width = widget.Width,
                    Height = widget.Height,
                    MinWidth = DesktopWidgetInstanceSettings.MinWidth,
                    MinHeight = DesktopWidgetInstanceSettings.MinHeight,
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

        return window;
    }

    private void SynchronizeOpenWindows(DesktopWidgetSettings settings)
    {
        if (windows.Count == 0)
        {
            return;
        }

        NormalizeSettingsForDisplay(settings);

        foreach (var windowEntry in windows.ToList())
        {
            var widget = settings.Widgets.FirstOrDefault(item => string.Equals(item.Id, windowEntry.Key, StringComparison.Ordinal));
            if (widget is null || !widget.IsEnabled || !DesktopWidgetCatalog.IsImplementedWidgetType(widget.WidgetType))
            {
                windowEntry.Value.Close();
                windows.Remove(windowEntry.Key);
                continue;
            }

            windowEntry.Value.Width = widget.Width;
            windowEntry.Value.Height = widget.Height;
            windowEntry.Value.CanResize = !settings.LockWidgetPositions;
            windowEntry.Value.Opacity = Math.Clamp(settings.GlobalOpacity, 0.2, 1.0);
            if (windowEntry.Value.DataContext is DesktopWidgetWindowViewModel viewModel)
            {
                viewModel.UpdateVisualSettings(settings.ShowWidgetBackground, settings.GlobalOpacity, settings.LockWidgetPositions);
            }
        }

        if (settings.EnableWidgets)
        {
            ShowWidgets();
        }
    }

    private void NormalizeSettingsForDisplay(DesktopWidgetSettings settings)
    {
        var (width, height) = GetVisibleArea();
        settings.NormalizeForVisibleArea(width, height);
    }

    private (double Width, double Height) GetVisibleArea()
    {
        var firstWindow = windows.Values.FirstOrDefault();
        var workingArea = firstWindow?.Screens.Primary?.WorkingArea;
        if (workingArea is null)
        {
            return (1920, 1080);
        }

        return (workingArea.Value.Width, workingArea.Value.Height);
    }

    private static double SnapToEdge(double value)
    {
        const double snapDistance = 16;
        return value <= snapDistance ? 0 : value;
    }
}
