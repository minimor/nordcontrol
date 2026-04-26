using NordControl.Core.Models;

namespace NordControl.Core.Modules;

public static class TaskbarPresetCatalog
{
    public const string DefaultPresetKey = "fluent-transparent";

    public static IReadOnlyList<TaskbarPreset> Presets { get; } =
    [
        new()
        {
            Key = "fluent-transparent",
            Name = "Fluent Transparent",
            Description = "A clean translucent bar with a cool accent line and centered app blocks.",
            VisualStyle = "Soft acrylic preview",
            RiskLevel = "Preview-only",
            AccentColorHex = "#4CC2FF",
            IsPreviewOnly = true,
            RequiresExplorerRestart = false,
            IsImplemented = false
        },
        new()
        {
            Key = "glass-floating",
            Name = "Glass Floating",
            Description = "A detached glass shelf concept with rounded tray and app islands.",
            VisualStyle = "Floating glass",
            RiskLevel = "Risk Lab future",
            AccentColorHex = "#8CF5D2",
            IsPreviewOnly = true,
            RequiresExplorerRestart = false,
            IsImplemented = false
        },
        new()
        {
            Key = "compact-focus",
            Name = "Compact Focus",
            Description = "A tighter utility layout for focused work, with restrained highlights.",
            VisualStyle = "Dense utility",
            RiskLevel = "Preview-only",
            AccentColorHex = "#A7B2C2",
            IsPreviewOnly = true,
            RequiresExplorerRestart = false,
            IsImplemented = false
        },
        new()
        {
            Key = "productivity-bar",
            Name = "Productivity Bar",
            Description = "A balanced taskbar mockup with readable tray status and workspace rhythm.",
            VisualStyle = "Work dashboard",
            RiskLevel = "Preview-only",
            AccentColorHex = "#FFD166",
            IsPreviewOnly = true,
            RequiresExplorerRestart = false,
            IsImplemented = false
        },
        new()
        {
            Key = "cyber-neon",
            Name = "Cyber Neon",
            Description = "A high-contrast neon concept for expressive desktop setups.",
            VisualStyle = "Neon edge glow",
            RiskLevel = "Preview-only",
            AccentColorHex = "#FF4FD8",
            IsPreviewOnly = true,
            RequiresExplorerRestart = false,
            IsImplemented = false
        },
        new()
        {
            Key = "minimal-dark",
            Name = "Minimal Dark",
            Description = "A quiet dark bar with minimal chrome and a single crisp accent.",
            VisualStyle = "Minimal matte",
            RiskLevel = "Preview-only",
            AccentColorHex = "#8FB7FF",
            IsPreviewOnly = true,
            RequiresExplorerRestart = false,
            IsImplemented = false
        }
    ];

    public static bool IsKnownPresetKey(string? key)
    {
        return Presets.Any(preset => string.Equals(preset.Key, key, StringComparison.Ordinal));
    }

    public static TaskbarPreset GetPresetOrDefault(string? key)
    {
        return Presets.FirstOrDefault(preset => string.Equals(preset.Key, key, StringComparison.Ordinal))
            ?? Presets.First(preset => preset.Key == DefaultPresetKey);
    }
}
