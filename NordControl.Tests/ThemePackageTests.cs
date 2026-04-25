using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Core.Services;

namespace NordControl.Tests;

public sealed class ThemePackageTests
{
    [Fact]
    public void CatalogContainsRequiredThemePackages()
    {
        var themeNames = ThemePackageCatalog.BuiltInThemes
            .Select(theme => theme.Name)
            .ToArray();

        Assert.Contains("Fluent Dark", themeNames);
        Assert.Contains("Glass Blue", themeNames);
        Assert.Contains("Graphite", themeNames);
        Assert.Contains("Aurora", themeNames);
        Assert.Contains("Cyber", themeNames);
        Assert.Contains("Nord Minimal", themeNames);
        Assert.Contains("Deep Space", themeNames);
    }

    [Fact]
    public void CatalogUsesStableUniqueKeysAndValidColors()
    {
        var themes = ThemePackageCatalog.BuiltInThemes;
        var keys = themes.Select(theme => theme.Key).ToArray();

        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.All(themes, theme =>
        {
            Assert.True(HexColorValidator.IsValidHexColor(theme.AccentColorHex));
            Assert.True(HexColorValidator.IsValidHexColor(theme.SecondaryAccentColorHex));
            Assert.True(HexColorValidator.IsValidHexColor(theme.BackgroundColorHex));
            Assert.True(HexColorValidator.IsValidHexColor(theme.SurfaceColorHex));
            Assert.True(HexColorValidator.IsValidHexColor(theme.TextColorHex));
            Assert.True(HexColorValidator.IsValidHexColor(theme.MutedTextColorHex));
            Assert.True(HexColorValidator.IsValidHexColor(theme.BorderColorHex));
        });
    }

    [Fact]
    public void NormalizerClampsInvalidValuesAndReplacesInvalidColors()
    {
        var theme = new ThemePackage
        {
            Key = " My Theme! ",
            Name = "",
            AccentColorHex = "blue",
            SecondaryAccentColorHex = "#12345Z",
            BackgroundColorHex = "",
            SurfaceColorHex = "transparent",
            TextColorHex = "#1234567",
            MutedTextColorHex = "#GGGGGG",
            BorderColorHex = "none",
            GlassOpacity = 3.5,
            CornerRadius = -8,
            Tags = ["", " utility ", "Utility"]
        };

        var normalized = ThemePackageNormalizer.Normalize(theme);

        Assert.Equal("my-theme", normalized.Key);
        Assert.Equal("Untitled Theme", normalized.Name);
        Assert.Equal("#4CC2FF", normalized.AccentColorHex);
        Assert.Equal("#8CF5D2", normalized.SecondaryAccentColorHex);
        Assert.Equal("#101418", normalized.BackgroundColorHex);
        Assert.Equal("#161D24", normalized.SurfaceColorHex);
        Assert.Equal("#F4F7FA", normalized.TextColorHex);
        Assert.Equal("#8FA1B3", normalized.MutedTextColorHex);
        Assert.Equal("#273442", normalized.BorderColorHex);
        Assert.Equal(1, normalized.GlassOpacity);
        Assert.Equal(0, normalized.CornerRadius);
        Assert.Single(normalized.Tags);
        Assert.Equal("utility", normalized.Tags[0]);
    }

    [Fact]
    public void ServiceExportsAndImportsThemePackageRoundTrip()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var service = new JsonThemePackageService();
            var filePath = Path.Combine(tempDirectory, "themes", "aurora.json");
            var theme = ThemePackageCatalog.GetThemeOrDefault("aurora");

            var exportResult = service.ExportTheme(theme, filePath);
            var importResult = service.ImportTheme(filePath, out var importedTheme);

            Assert.True(exportResult.Success);
            Assert.True(File.Exists(filePath));
            Assert.True(importResult.Success);
            Assert.NotNull(importedTheme);
            Assert.Equal(theme.Key, importedTheme.Key);
            Assert.Equal(theme.AccentColorHex, importedTheme.AccentColorHex);
            Assert.Equal(theme.Tags, importedTheme.Tags);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void ServiceReturnsFriendlyFailureForInvalidJson()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var filePath = Path.Combine(tempDirectory, "broken.json");
            File.WriteAllText(filePath, "{not valid json");

            var service = new JsonThemePackageService();
            var result = service.ImportTheme(filePath, out var theme);

            Assert.False(result.Success);
            Assert.Null(theme);
            Assert.Contains("invalid", result.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"NordControl.ThemeTests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
