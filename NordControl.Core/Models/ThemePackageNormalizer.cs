using System.Text.RegularExpressions;

namespace NordControl.Core.Models;

public static partial class ThemePackageNormalizer
{
    private const double MinimumGlassOpacity = 0;
    private const double MaximumGlassOpacity = 1;
    private const double MinimumCornerRadius = 0;
    private const double MaximumCornerRadius = 32;

    public static ThemePackage Normalize(ThemePackage? theme)
    {
        theme ??= new ThemePackage();

        theme.Key = NormalizeKey(theme.Key);
        theme.Name = NormalizeText(theme.Name, "Untitled Theme");
        theme.Description = NormalizeText(theme.Description, "A NordControl theme package.");
        theme.Author = NormalizeText(theme.Author, "NordControl");
        theme.Version = NormalizeText(theme.Version, "1.0.0");
        theme.AccentColorHex = NormalizeColor(theme.AccentColorHex, "#4CC2FF");
        theme.SecondaryAccentColorHex = NormalizeColor(theme.SecondaryAccentColorHex, "#8CF5D2");
        theme.BackgroundColorHex = NormalizeColor(theme.BackgroundColorHex, "#101418");
        theme.SurfaceColorHex = NormalizeColor(theme.SurfaceColorHex, "#161D24");
        theme.TextColorHex = NormalizeColor(theme.TextColorHex, "#F4F7FA");
        theme.MutedTextColorHex = NormalizeColor(theme.MutedTextColorHex, "#8FA1B3");
        theme.BorderColorHex = NormalizeColor(theme.BorderColorHex, "#273442");
        theme.GlassOpacity = Math.Clamp(theme.GlassOpacity, MinimumGlassOpacity, MaximumGlassOpacity);
        theme.CornerRadius = Math.Clamp(theme.CornerRadius, MinimumCornerRadius, MaximumCornerRadius);
        theme.Mood = NormalizeText(theme.Mood, "Focused");
        theme.Tags = NormalizeTags(theme.Tags);

        return theme;
    }

    private static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "custom-theme";
        }

        var key = value.Trim().ToLowerInvariant();
        key = InvalidKeyCharacters().Replace(key, "-").Trim('-');
        return string.IsNullOrWhiteSpace(key) ? "custom-theme" : key;
    }

    private static string NormalizeText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string NormalizeColor(string? value, string fallback)
    {
        return HexColorValidator.IsValidHexColor(value) ? value!.ToUpperInvariant() : fallback;
    }

    private static List<string> NormalizeTags(IEnumerable<string>? tags)
    {
        var normalized = (tags ?? [])
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalized.Count > 0 ? normalized : ["custom"];
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex InvalidKeyCharacters();
}
