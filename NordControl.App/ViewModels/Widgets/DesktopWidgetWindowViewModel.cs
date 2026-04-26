using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using NordControl.App.ViewModels;
using NordControl.Core.Models;
using NordControl.Core.Modules;

namespace NordControl.App.ViewModels.Widgets;

public partial class DesktopWidgetWindowViewModel : ViewModelBase, IDisposable
{
    private readonly DispatcherTimer timer;
    private readonly EventHandler tickHandler;

    public DesktopWidgetWindowViewModel(
        DesktopWidgetInstanceSettings settings,
        DesktopWidgetDefinition definition,
        bool showBackground,
        double globalOpacity)
    {
        Settings = settings;
        Definition = definition;
        AccentColorHex = definition.AccentColorHex;
        WidgetName = definition.Name;
        ShowBackground = showBackground;
        EffectiveOpacity = Math.Clamp(settings.Opacity * globalOpacity, 0.2, 1.0);
        IsClockWidget = string.Equals(settings.WidgetType, DesktopWidgetCatalog.ClockWidgetKey, StringComparison.Ordinal);
        IsSystemMonitorLiteWidget = string.Equals(settings.WidgetType, DesktopWidgetCatalog.SystemMonitorLiteWidgetKey, StringComparison.Ordinal);

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        tickHandler = (_, _) => Refresh();
        timer.Tick += tickHandler;
        Refresh();
        timer.Start();
    }

    public DesktopWidgetInstanceSettings Settings { get; }

    public DesktopWidgetDefinition Definition { get; }

    public bool IsClockWidget { get; }

    public bool IsSystemMonitorLiteWidget { get; }

    [ObservableProperty]
    private string widgetName = "Widget";

    [ObservableProperty]
    private string accentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool showBackground = true;

    [ObservableProperty]
    private double effectiveOpacity = 0.92;

    [ObservableProperty]
    private string currentTime = "--:--";

    [ObservableProperty]
    private string currentDate = "Loading";

    [ObservableProperty]
    private string cpuText = "CPU ready";

    [ObservableProperty]
    private string memoryText = "Memory ready";

    [ObservableProperty]
    private string monitorNote = "Safe live metrics provider planned.";

    public string WidgetBackgroundColor => ShowBackground ? "#E617202A" : "#6617202A";

    public void Dispose()
    {
        timer.Stop();
        timer.Tick -= tickHandler;
    }

    private void Refresh()
    {
        var now = DateTime.Now;
        CurrentTime = now.ToString("HH:mm:ss");
        CurrentDate = now.ToString("dddd, MMMM d");
        CpuText = "CPU: placeholder";
        MemoryText = $"App memory: {GC.GetTotalMemory(forceFullCollection: false) / 1024 / 1024} MB";
        MonitorNote = "System-wide metrics will be added behind a lightweight provider.";
    }
}
