using NordControl.Core.Modules;

namespace NordControl.Core.Models;

public sealed class DesktopWidgetSettings
{
    public bool EnableWidgets { get; set; }

    public bool StartWidgetsWithApp { get; set; }

    public bool LockWidgetPositions { get; set; }

    public bool ShowWidgetBackground { get; set; } = true;

    public double GlobalOpacity { get; set; } = 0.92;

    public string SelectedWidgetThemeKey { get; set; } = DesktopWidgetCatalog.DefaultWidgetThemeKey;

    public List<DesktopWidgetInstanceSettings> Widgets { get; set; } = DesktopWidgetCatalog.CreateDefaultWidgetInstances();

    public void Normalize()
    {
        GlobalOpacity = ClampFinite(GlobalOpacity, 0.2, 1.0);

        if (string.IsNullOrWhiteSpace(SelectedWidgetThemeKey))
        {
            SelectedWidgetThemeKey = DesktopWidgetCatalog.DefaultWidgetThemeKey;
        }

        Widgets ??= [];
        if (Widgets.Count == 0)
        {
            Widgets = DesktopWidgetCatalog.CreateDefaultWidgetInstances();
        }

        foreach (var widget in Widgets)
        {
            widget.Normalize();
        }

        EnsureDefaultWidget(DesktopWidgetCatalog.ClockWidgetKey);
        EnsureDefaultWidget(DesktopWidgetCatalog.SystemMonitorLiteWidgetKey);
    }

    private void EnsureDefaultWidget(string widgetType)
    {
        if (Widgets.Any(widget => string.Equals(widget.WidgetType, widgetType, StringComparison.Ordinal)))
        {
            return;
        }

        Widgets.Add(DesktopWidgetCatalog.CreateDefaultInstance(widgetType));
    }

    private static double ClampFinite(double value, double min, double max)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return min;
        }

        return Math.Clamp(value, min, max);
    }
}
