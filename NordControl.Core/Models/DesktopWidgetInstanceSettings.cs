using NordControl.Core.Modules;

namespace NordControl.Core.Models;

public sealed class DesktopWidgetInstanceSettings
{
    public const double MinWidth = 180;
    public const double MaxWidth = 900;
    public const double MinHeight = 110;
    public const double MaxHeight = 700;

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
        Width = ClampFinite(Width, MinWidth, MaxWidth);
        Height = ClampFinite(Height, MinHeight, MaxHeight);
        Opacity = ClampFinite(Opacity, 0.2, 1.0);
    }

    public void NormalizeForVisibleArea(double visibleWidth, double visibleHeight)
    {
        Normalize();

        if (visibleWidth <= 0 || double.IsNaN(visibleWidth) || double.IsInfinity(visibleWidth))
        {
            visibleWidth = 1280;
        }

        if (visibleHeight <= 0 || double.IsNaN(visibleHeight) || double.IsInfinity(visibleHeight))
        {
            visibleHeight = 720;
        }

        Width = Math.Min(Width, Math.Max(MinWidth, visibleWidth));
        Height = Math.Min(Height, Math.Max(MinHeight, visibleHeight));

        var maxX = Math.Max(0, visibleWidth - Math.Min(80, Width));
        var maxY = Math.Max(0, visibleHeight - Math.Min(80, Height));
        X = ClampFinite(X, 0, maxX);
        Y = ClampFinite(Y, 0, maxY);
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
