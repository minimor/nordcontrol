using NordControl.Core.Models;

namespace NordControl.Core.Modules;

public static class CustomizationPresetCatalog
{
    public static IReadOnlyList<CustomizationPreset> DefaultPresets { get; } =
    [
        new(
            "fluent-dark",
            "Fluent Dark",
            "A calm dark utility look with crisp blue accents.",
            "#4CC2FF",
            "Soft dark glass",
            "Focused",
            true,
            "App-only"),
        new(
            "glass-blue",
            "Glass Blue",
            "A cool translucent control-room palette for bright highlights.",
            "#72D7FF",
            "Acrylic blue panels",
            "Airy",
            true,
            "App-only"),
        new(
            "graphite",
            "Graphite",
            "Quiet graphite surfaces with restrained silver-blue detail.",
            "#A7B2C2",
            "Graphite layers",
            "Minimal",
            true,
            "App-only"),
        new(
            "aurora",
            "Aurora",
            "A luminous teal and violet mix for a more expressive desktop mood.",
            "#8CF5D2",
            "Aurora wash",
            "Expressive",
            true,
            "App-only"),
        new(
            "cyber",
            "Cyber",
            "High-contrast neon accents for a command-center feel.",
            "#FF4FD8",
            "Neon contrast",
            "Electric",
            true,
            "App-only")
    ];
}
