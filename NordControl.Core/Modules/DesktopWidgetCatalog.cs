using NordControl.Core.Models;

namespace NordControl.Core.Modules;

public static class DesktopWidgetCatalog
{
    public const string ClockWidgetKey = "clock";
    public const string SystemMonitorLiteWidgetKey = "system-monitor-lite";
    public const string DefaultWidgetThemeKey = "nord-glass";

    public static IReadOnlyList<DesktopWidgetDefinition> Definitions { get; } =
    [
        new()
        {
            Key = ClockWidgetKey,
            Name = "Clock",
            Description = "A clean desktop clock with current time and date.",
            Category = "Essentials",
            RiskLevel = "Low",
            IsImplemented = true,
            AccentColorHex = "#4CC2FF"
        },
        new()
        {
            Key = SystemMonitorLiteWidgetKey,
            Name = "System Monitor Lite",
            Description = "A lightweight status card prepared for safe CPU and memory metrics.",
            Category = "System",
            RiskLevel = "Low",
            IsImplemented = true,
            AccentColorHex = "#8CF5D2"
        },
        new()
        {
            Key = "music-controls",
            Name = "Music Controls",
            Description = "A future media control card for playback and current track status.",
            Category = "Media",
            RiskLevel = "Safe preview",
            IsImplemented = false,
            AccentColorHex = "#A78BFA"
        },
        new()
        {
            Key = "quick-notes",
            Name = "Quick Notes",
            Description = "A future small notes surface for scratchpad workflows.",
            Category = "Productivity",
            RiskLevel = "Safe preview",
            IsImplemented = false,
            AccentColorHex = "#FFD166"
        },
        new()
        {
            Key = "shortcuts-panel",
            Name = "Shortcuts Panel",
            Description = "A future launcher strip for apps, folders, and workspaces.",
            Category = "Launcher",
            RiskLevel = "Safe preview",
            IsImplemented = false,
            AccentColorHex = "#FF8FB3"
        },
        new()
        {
            Key = "weather",
            Name = "Weather",
            Description = "A planned glanceable weather module.",
            Category = "Info",
            RiskLevel = "Safe preview",
            IsImplemented = false,
            AccentColorHex = "#72D7FF"
        },
        new()
        {
            Key = "performance-monitor",
            Name = "Performance Monitor",
            Description = "A richer future performance module with historical charts.",
            Category = "System",
            RiskLevel = "Medium future",
            IsImplemented = false,
            AccentColorHex = "#FF4FD8"
        }
    ];

    public static bool IsKnownWidgetType(string? widgetType)
    {
        return Definitions.Any(definition => string.Equals(definition.Key, widgetType, StringComparison.Ordinal));
    }

    public static bool IsImplementedWidgetType(string? widgetType)
    {
        return Definitions.Any(definition =>
            string.Equals(definition.Key, widgetType, StringComparison.Ordinal) &&
            definition.IsImplemented);
    }

    public static DesktopWidgetDefinition GetDefinitionOrDefault(string? widgetType)
    {
        return Definitions.FirstOrDefault(definition => string.Equals(definition.Key, widgetType, StringComparison.Ordinal))
            ?? Definitions.First(definition => definition.Key == ClockWidgetKey);
    }

    public static List<DesktopWidgetInstanceSettings> CreateDefaultWidgetInstances()
    {
        return
        [
            CreateDefaultInstance(ClockWidgetKey),
            CreateDefaultInstance(SystemMonitorLiteWidgetKey)
        ];
    }

    public static DesktopWidgetInstanceSettings CreateDefaultInstance(string widgetType)
    {
        return widgetType switch
        {
            SystemMonitorLiteWidgetKey => new DesktopWidgetInstanceSettings
            {
                Id = "system-monitor-lite-default",
                WidgetType = SystemMonitorLiteWidgetKey,
                IsEnabled = true,
                X = 80,
                Y = 250,
                Width = 300,
                Height = 170,
                IsAlwaysOnTop = true,
                Opacity = 0.92
            },
            _ => new DesktopWidgetInstanceSettings
            {
                Id = "clock-default",
                WidgetType = ClockWidgetKey,
                IsEnabled = true,
                X = 80,
                Y = 80,
                Width = 300,
                Height = 150,
                IsAlwaysOnTop = true,
                Opacity = 0.92
            }
        };
    }
}
