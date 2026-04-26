using NordControl.Core.Models;
using NordControl.Core.Modules;

namespace NordControl.Tests;

public sealed class DesktopWidgetTests
{
    [Fact]
    public void WidgetCatalogContainsRequiredDefinitions()
    {
        var names = DesktopWidgetCatalog.Definitions
            .Select(definition => definition.Name)
            .ToArray();

        Assert.Contains("Clock", names);
        Assert.Contains("System Monitor Lite", names);
        Assert.Contains("Music Controls", names);
        Assert.Contains("Quick Notes", names);
        Assert.Contains("Shortcuts Panel", names);
        Assert.Contains("Weather", names);
        Assert.Contains("Performance Monitor", names);
    }

    [Fact]
    public void WidgetCatalogMarksOnlyV1WidgetsImplemented()
    {
        Assert.True(DesktopWidgetCatalog.IsImplementedWidgetType("clock"));
        Assert.True(DesktopWidgetCatalog.IsImplementedWidgetType("system-monitor-lite"));
        Assert.False(DesktopWidgetCatalog.IsImplementedWidgetType("music-controls"));
    }

    [Fact]
    public void DesktopWidgetSettingsNormalizeOpacityAndDefaultWidgets()
    {
        var settings = new DesktopWidgetSettings
        {
            GlobalOpacity = 1.8,
            SelectedWidgetThemeKey = "",
            Widgets = []
        };

        settings.Normalize();

        Assert.Equal(1.0, settings.GlobalOpacity);
        Assert.Equal(DesktopWidgetCatalog.DefaultWidgetThemeKey, settings.SelectedWidgetThemeKey);
        Assert.Contains(settings.Widgets, widget => widget.WidgetType == DesktopWidgetCatalog.ClockWidgetKey);
        Assert.Contains(settings.Widgets, widget => widget.WidgetType == DesktopWidgetCatalog.SystemMonitorLiteWidgetKey);
    }

    [Theory]
    [InlineData(0.05, 0.2)]
    [InlineData(0.75, 0.75)]
    [InlineData(4.0, 1.0)]
    public void WidgetInstanceNormalizeClampsOpacity(double input, double expected)
    {
        var settings = new DesktopWidgetInstanceSettings
        {
            Opacity = input
        };

        settings.Normalize();

        Assert.Equal(expected, settings.Opacity);
    }

    [Fact]
    public void WidgetInstanceNormalizeClampsSizeToReasonableBounds()
    {
        var settings = new DesktopWidgetInstanceSettings
        {
            Width = 40,
            Height = 2000
        };

        settings.Normalize();

        Assert.Equal(DesktopWidgetInstanceSettings.MinWidth, settings.Width);
        Assert.Equal(DesktopWidgetInstanceSettings.MaxHeight, settings.Height);
    }

    [Fact]
    public void WidgetInstanceNormalizeForVisibleAreaKeepsWidgetReachable()
    {
        var settings = new DesktopWidgetInstanceSettings
        {
            X = 5000,
            Y = 4000,
            Width = 420,
            Height = 260
        };

        settings.NormalizeForVisibleArea(1024, 768);

        Assert.InRange(settings.X, 0, 1024);
        Assert.InRange(settings.Y, 0, 768);
        Assert.True(settings.X <= 944);
        Assert.True(settings.Y <= 688);
    }

    [Fact]
    public void DesktopWidgetSettingsNormalizeForVisibleAreaAppliesToAllWidgets()
    {
        var settings = new DesktopWidgetSettings
        {
            Widgets =
            [
                new DesktopWidgetInstanceSettings
                {
                    Id = "clock",
                    WidgetType = DesktopWidgetCatalog.ClockWidgetKey,
                    X = 9999,
                    Y = 9999
                },
                new DesktopWidgetInstanceSettings
                {
                    Id = "system",
                    WidgetType = DesktopWidgetCatalog.SystemMonitorLiteWidgetKey,
                    X = 9999,
                    Y = 9999
                }
            ]
        };

        settings.NormalizeForVisibleArea(800, 600);

        Assert.All(settings.Widgets, widget =>
        {
            Assert.True(widget.X <= 720);
            Assert.True(widget.Y <= 520);
        });
    }

    [Fact]
    public void DefaultWidgetLayoutUsesStableImplementedWidgets()
    {
        var widgets = DesktopWidgetCatalog.CreateDefaultWidgetInstances();

        Assert.Equal(2, widgets.Count);
        Assert.Equal("clock-default", widgets[0].Id);
        Assert.Equal("system-monitor-lite-default", widgets[1].Id);
        Assert.All(widgets, widget => Assert.True(DesktopWidgetCatalog.IsImplementedWidgetType(widget.WidgetType)));
    }

    [Fact]
    public void UnknownWidgetTypesAreDisabledSafely()
    {
        var settings = new DesktopWidgetSettings
        {
            Widgets =
            [
                new DesktopWidgetInstanceSettings
                {
                    Id = "unknown-widget",
                    WidgetType = "not-real",
                    IsEnabled = true,
                    Width = 40,
                    Height = 20
                }
            ]
        };

        settings.Normalize();

        var unknown = settings.Widgets.Single(widget => widget.WidgetType == "not-real");
        Assert.False(unknown.IsEnabled);
        Assert.True(unknown.Width >= 180);
        Assert.True(unknown.Height >= 110);
    }

    [Fact]
    public void WidgetOperationResultFactoriesSetExpectedState()
    {
        var success = WidgetOperationResult.Succeeded("Shown.");
        var failure = WidgetOperationResult.Failed("Failed.", "Window error");

        Assert.True(success.Success);
        Assert.Equal("Shown.", success.Message);
        Assert.False(failure.Success);
        Assert.Equal("Window error", failure.Details);
    }
}
