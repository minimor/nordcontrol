using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NordControl.App.ViewModels;
using NordControl.Core.Models;
using NordControl.Core.Modules;

namespace NordControl.App.ViewModels.Widgets;

public partial class DesktopWidgetWindowViewModel : ViewModelBase, IDisposable
{
    private readonly DispatcherTimer timer;
    private readonly EventHandler tickHandler;
    private readonly Func<string, double, double, double, double, WidgetOperationResult> saveBounds;
    private readonly Action<string> hideWidget;
    private DateTime lastCpuSampleAt = DateTime.Now;
    private TimeSpan lastProcessorTime = TimeSpan.Zero;

    public DesktopWidgetWindowViewModel(
        DesktopWidgetInstanceSettings settings,
        DesktopWidgetDefinition definition,
        bool showBackground,
        double globalOpacity,
        bool lockWidgetPositions,
        Func<string, double, double, double, double, WidgetOperationResult> saveBounds,
        Action<string> hideWidget)
    {
        Settings = settings;
        Definition = definition;
        this.saveBounds = saveBounds;
        this.hideWidget = hideWidget;
        AccentColorHex = definition.AccentColorHex;
        WidgetName = definition.Name;
        ShowBackground = showBackground;
        LockWidgetPositions = lockWidgetPositions;
        EffectiveOpacity = Math.Clamp(settings.Opacity * globalOpacity, 0.2, 1.0);
        IsClockWidget = string.Equals(settings.WidgetType, DesktopWidgetCatalog.ClockWidgetKey, StringComparison.Ordinal);
        IsSystemMonitorLiteWidget = string.Equals(settings.WidgetType, DesktopWidgetCatalog.SystemMonitorLiteWidgetKey, StringComparison.Ordinal);
        CloseWidgetCommand = new RelayCommand(CloseWidget);
        lastProcessorTime = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;

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

    public IRelayCommand CloseWidgetCommand { get; }

    [ObservableProperty]
    private string widgetName = "Widget";

    [ObservableProperty]
    private string accentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool showBackground = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LockStatusText))]
    [NotifyPropertyChangedFor(nameof(DragHintText))]
    private bool lockWidgetPositions;

    [ObservableProperty]
    private double effectiveOpacity = 0.92;

    [ObservableProperty]
    private string currentTime = "--:--";

    [ObservableProperty]
    private string currentDate = "Loading";

    [ObservableProperty]
    private string currentDay = "Loading";

    [ObservableProperty]
    private string cpuText = "CPU ready";

    [ObservableProperty]
    private string memoryText = "Memory ready";

    [ObservableProperty]
    private string monitorNote = "Safe live metrics provider planned.";

    [ObservableProperty]
    private string memoryDetailText = "Memory details loading";

    public string WidgetBackgroundColor => ShowBackground ? "#E617202A" : "#6617202A";

    public string LockStatusText => LockWidgetPositions ? "Locked" : "Unlocked";

    public string DragHintText => LockWidgetPositions ? "Position locked" : "Drag header to move";

    public void UpdateVisualSettings(bool showBackground, double globalOpacity, bool lockWidgetPositions)
    {
        ShowBackground = showBackground;
        LockWidgetPositions = lockWidgetPositions;
        EffectiveOpacity = Math.Clamp(Settings.Opacity * globalOpacity, 0.2, 1.0);
        OnPropertyChanged(nameof(WidgetBackgroundColor));
    }

    public void SaveBounds(double x, double y, double width, double height)
    {
        saveBounds(Settings.Id, x, y, width, height);
    }

    public void Dispose()
    {
        timer.Stop();
        timer.Tick -= tickHandler;
    }

    private void Refresh()
    {
        var now = DateTime.Now;
        CurrentTime = now.ToString("HH:mm:ss");
        CurrentDate = now.ToString("MMMM d, yyyy");
        CurrentDay = now.ToString("dddd");
        CpuText = GetAppCpuText(now);
        MemoryText = $"App memory: {GC.GetTotalMemory(forceFullCollection: false) / 1024 / 1024:N0} MB";
        MemoryDetailText = GetMemoryDetailText();
        MonitorNote = "App-owned overlay. System-wide CPU provider planned.";
    }

    private void CloseWidget()
    {
        hideWidget(Settings.Id);
    }

    private string GetAppCpuText(DateTime now)
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        var processorTime = process.TotalProcessorTime;
        var elapsed = now - lastCpuSampleAt;
        var cpuElapsed = processorTime - lastProcessorTime;

        lastCpuSampleAt = now;
        lastProcessorTime = processorTime;

        if (elapsed.TotalMilliseconds <= 0)
        {
            return "App CPU: warming up";
        }

        var percent = cpuElapsed.TotalMilliseconds / (elapsed.TotalMilliseconds * Environment.ProcessorCount) * 100;
        return $"App CPU: {Math.Clamp(percent, 0, 100):N1}%";
    }

    private static string GetMemoryDetailText()
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        if (memoryInfo.TotalAvailableMemoryBytes <= 0)
        {
            return "RAM scope: app metrics";
        }

        return $"GC memory budget: {memoryInfo.TotalAvailableMemoryBytes / 1024 / 1024:N0} MB";
    }
}
