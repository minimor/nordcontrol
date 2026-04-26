using NordControl.Core.Modules;

namespace NordControl.Core.Models;

public sealed class DesktopWidgetInstanceSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string WidgetType { get; set; } = DesktopWidgetCatalog.ClockWidgetKey;

    public bool IsEnabled { get; set; } = true;

    public double X { get; set; } = 80;

    public double Y { get; set; } = 80;

    public double Width { get; set; } = 280;

    public double Height { get; set; } = 150;

    public bool IsAlwaysOnTop { get; set; } = true;

    public double Opacity { get; set; } = 0.92;

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Id = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrWhiteSpace(WidgetType))
        {
            WidgetType = DesktopWidgetCatalog.ClockWidgetKey;
        }

        if (!DesktopWidgetCatalog.IsKnownWidgetType(WidgetType))
        {
            IsEnabled = false;
        }

        X = ClampFinite(X, 0, 10000);
        Y = ClampFinite(Y, 0, 10000);
        Width = ClampFinite(Width, 180, 900);
        Height = ClampFinite(Height, 110, 700);
        Opacity = ClampFinite(Opacity, 0.2, 1.0);
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
