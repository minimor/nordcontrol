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
            var userThemesDirectory = Path.Combine(tempDirectory, "user-themes");
            var service = new JsonThemePackageService(userThemesDirectory);
            var filePath = Path.Combine(tempDirectory, "export", "round-trip.json");
            var theme = new ThemePackage
            {
                Key = "round-trip",
                Name = "Round Trip",
                AccentColorHex = "#12ABEF",
                BackgroundColorHex = "#101418",
                SurfaceColorHex = "#161D24",
                TextColorHex = "#F4F7FA"
            };

            var exportResult = service.ExportPackage(theme, filePath);
            var importResult = service.ImportPackage(filePath);
            var importedTheme = service.GetUserPackages().Single();

            Assert.True(exportResult.Success);
            Assert.True(File.Exists(filePath));
            Assert.True(importResult.Success);
            Assert.Equal(theme.Key, importedTheme.Key);
            Assert.Equal("#12ABEF", importedTheme.AccentColorHex);
            Assert.Contains(service.GetAllPackages(), package => package.Key == importedTheme.Key);
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

            var service = new JsonThemePackageService(Path.Combine(tempDirectory, "themes"));
            var result = service.ImportPackage(filePath);

            Assert.False(result.Success);
            Assert.Contains("invalid", result.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void ServicePersistsUserPackagesAcrossInstances()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var service = new JsonThemePackageService(tempDirectory);
            var theme = new ThemePackage
            {
                Key = "saved-theme",
                Name = "Saved Theme",
                AccentColorHex = "#AABBCC"
            };

            var saveResult = service.SaveUserPackage(theme);
            var reloadedService = new JsonThemePackageService(tempDirectory);
            var userThemes = reloadedService.GetUserPackages();

            Assert.True(saveResult.Success);
            Assert.Single(userThemes);
            Assert.Equal("saved-theme", userThemes[0].Key);
            Assert.Equal("Saved Theme", userThemes[0].Name);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void ImportDoesNotOverwriteBuiltInPackageKeys()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var importFilePath = Path.Combine(tempDirectory, "fluent-dark.json");
            var service = new JsonThemePackageService(Path.Combine(tempDirectory, "themes"));
            var exportResult = service.ExportPackage(ThemePackageCatalog.DefaultTheme, importFilePath);
            var importResult = service.ImportPackage(importFilePath);
            var userTheme = service.GetUserPackages().Single();

            Assert.True(exportResult.Success);
            Assert.True(importResult.Success);
            Assert.Equal("user-fluent-dark", userTheme.Key);
            Assert.Contains(ThemePackageCatalog.DefaultTheme, service.GetBuiltInPackages());
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
