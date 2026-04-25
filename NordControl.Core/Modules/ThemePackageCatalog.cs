using NordControl.Core.Models;

namespace NordControl.Core.Modules;

public static class ThemePackageCatalog
{
    public const string DefaultThemePackageKey = "fluent-dark";

    public static IReadOnlyList<ThemePackage> BuiltInThemes { get; } =
    [
        ThemePackageNormalizer.Normalize(new ThemePackage
        {
            Key = "fluent-dark",
            Name = "Fluent Dark",
            Description = "A calm dark utility look with crisp blue accents and balanced contrast.",
            Author = "NordControl",
            Version = "1.0.0",
            AccentColorHex = "#4CC2FF",
            SecondaryAccentColorHex = "#8CF5D2",
            BackgroundColorHex = "#101418",
            SurfaceColorHex = "#161D24",
            TextColorHex = "#F4F7FA",
            MutedTextColorHex = "#8FA1B3",
            BorderColorHex = "#273442",
            EnableGlass = true,
            GlassOpacity = 0.72,
            CornerRadius = 8,
            Mood = "Focused",
            Tags = ["dark", "fluent", "utility"]
        }),
        ThemePackageNormalizer.Normalize(new ThemePackage
        {
            Key = "glass-blue",
            Name = "Glass Blue",
            Description = "A cool translucent control-room palette for bright highlights and airy panels.",
            Author = "NordControl",
            Version = "1.0.0",
            AccentColorHex = "#72D7FF",
            SecondaryAccentColorHex = "#B7D7F2",
            BackgroundColorHex = "#0B1620",
            SurfaceColorHex = "#142534",
            TextColorHex = "#F3FBFF",
            MutedTextColorHex = "#9FC6D8",
            BorderColorHex = "#2E5B73",
            EnableGlass = true,
            GlassOpacity = 0.52,
            CornerRadius = 12,
            Mood = "Airy",
            Tags = ["glass", "blue", "transparent"]
        }),
        ThemePackageNormalizer.Normalize(new ThemePackage
        {
            Key = "graphite",
            Name = "Graphite",
            Description = "Quiet graphite surfaces with restrained silver-blue detail for long sessions.",
            Author = "NordControl",
            Version = "1.0.0",
            AccentColorHex = "#A7B2C2",
            SecondaryAccentColorHex = "#6F7C8D",
            BackgroundColorHex = "#111315",
            SurfaceColorHex = "#1C2024",
            TextColorHex = "#EEF1F4",
            MutedTextColorHex = "#A0A8B2",
            BorderColorHex = "#343B43",
            EnableGlass = false,
            GlassOpacity = 0.18,
            CornerRadius = 6,
            Mood = "Minimal",
            Tags = ["neutral", "graphite", "solid"]
        }),
        ThemePackageNormalizer.Normalize(new ThemePackage
        {
            Key = "aurora",
            Name = "Aurora",
            Description = "A luminous teal and violet mix for a more expressive desktop mood.",
            Author = "NordControl",
            Version = "1.0.0",
            AccentColorHex = "#8CF5D2",
            SecondaryAccentColorHex = "#A78BFA",
            BackgroundColorHex = "#101821",
            SurfaceColorHex = "#17222E",
            TextColorHex = "#F6FFFB",
            MutedTextColorHex = "#A9C8C0",
            BorderColorHex = "#315B58",
            EnableGlass = true,
            GlassOpacity = 0.62,
            CornerRadius = 14,
            Mood = "Expressive",
            Tags = ["aurora", "teal", "violet"]
        }),
        ThemePackageNormalizer.Normalize(new ThemePackage
        {
            Key = "cyber",
            Name = "Cyber",
            Description = "High-contrast neon accents for a command-center feel.",
            Author = "NordControl",
            Version = "1.0.0",
            AccentColorHex = "#FF4FD8",
            SecondaryAccentColorHex = "#00F5FF",
            BackgroundColorHex = "#090912",
            SurfaceColorHex = "#171021",
            TextColorHex = "#FFF6FD",
            MutedTextColorHex = "#B98FB0",
            BorderColorHex = "#5A2353",
            EnableGlass = true,
            GlassOpacity = 0.48,
            CornerRadius = 4,
            Mood = "Electric",
            Tags = ["neon", "cyber", "high-contrast"]
        }),
        ThemePackageNormalizer.Normalize(new ThemePackage
        {
            Key = "nord-minimal",
            Name = "Nord Minimal",
            Description = "A quiet arctic-inspired palette with soft blue-gray surfaces and clean emphasis.",
            Author = "NordControl",
            Version = "1.0.0",
            AccentColorHex = "#88C0D0",
            SecondaryAccentColorHex = "#A3BE8C",
            BackgroundColorHex = "#121821",
            SurfaceColorHex = "#202A36",
            TextColorHex = "#ECEFF4",
            MutedTextColorHex = "#9AA9B8",
            BorderColorHex = "#3B4654",
            EnableGlass = false,
            GlassOpacity = 0.28,
            CornerRadius = 8,
            Mood = "Quiet",
            Tags = ["minimal", "cool", "balanced"]
        }),
        ThemePackageNormalizer.Normalize(new ThemePackage
        {
            Key = "deep-space",
            Name = "Deep Space",
            Description = "A darker cosmic package with blue-violet accents and soft starless depth.",
            Author = "NordControl",
            Version = "1.0.0",
            AccentColorHex = "#8FB7FF",
            SecondaryAccentColorHex = "#D980FA",
            BackgroundColorHex = "#080B16",
            SurfaceColorHex = "#11182A",
            TextColorHex = "#F0F4FF",
            MutedTextColorHex = "#9AA8CC",
            BorderColorHex = "#2B365D",
            EnableGlass = true,
            GlassOpacity = 0.58,
            CornerRadius = 10,
            Mood = "Immersive",
            Tags = ["space", "violet", "deep"]
        })
    ];

    public static ThemePackage DefaultTheme => BuiltInThemes.First(theme => theme.Key == DefaultThemePackageKey);

    public static bool IsKnownThemeKey(string? key)
    {
        return BuiltInThemes.Any(theme => string.Equals(theme.Key, key, StringComparison.Ordinal));
    }

    public static ThemePackage GetThemeOrDefault(string? key)
    {
        return BuiltInThemes.FirstOrDefault(theme => string.Equals(theme.Key, key, StringComparison.Ordinal))
            ?? DefaultTheme;
    }
}
