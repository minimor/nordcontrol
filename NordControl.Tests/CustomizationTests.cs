using NordControl.Core.Models;
using NordControl.Core.Modules;

namespace NordControl.Tests;

public sealed class CustomizationTests
{
    [Fact]
    public void PresetCatalogContainsRequiredPresets()
    {
        var presetNames = CustomizationPresetCatalog.DefaultPresets
            .Select(preset => preset.Name)
            .ToArray();

        Assert.Contains("Fluent Dark", presetNames);
        Assert.Contains("Glass Blue", presetNames);
        Assert.Contains("Graphite", presetNames);
        Assert.Contains("Aurora", presetNames);
        Assert.Contains("Cyber", presetNames);
    }

    [Fact]
    public void SectionCatalogContainsRequiredSections()
    {
        var sectionNames = CustomizationSectionCatalog.Sections
            .Select(section => section.Name)
            .ToArray();

        Assert.Contains("Overview", sectionNames);
        Assert.Contains("Themes", sectionNames);
        Assert.Contains("Taskbar", sectionNames);
        Assert.Contains("Start Menu", sectionNames);
        Assert.Contains("Desktop Widgets", sectionNames);
        Assert.Contains("Window Effects", sectionNames);
        Assert.Contains("Launcher", sectionNames);
        Assert.Contains("Layouts / Tiling", sectionNames);
        Assert.Contains("Advanced / Risk Lab", sectionNames);
    }

    [Fact]
    public void SectionCatalogUsesOverviewAsDefault()
    {
        Assert.Equal("overview", CustomizationSectionCatalog.DefaultSectionKey);
        Assert.True(CustomizationSectionCatalog.IsKnownSectionKey("overview"));
        Assert.False(CustomizationSectionCatalog.IsKnownSectionKey("missing"));
    }

    [Fact]
    public void SectionCatalogProvidesFeatureCardsForPlanningSections()
    {
        Assert.NotEmpty(CustomizationSectionCatalog.GetFeatureCards("taskbar"));
        Assert.NotEmpty(CustomizationSectionCatalog.GetFeatureCards("start-menu"));
        Assert.NotEmpty(CustomizationSectionCatalog.GetFeatureCards("desktop-widgets"));
        Assert.NotEmpty(CustomizationSectionCatalog.GetFeatureCards("window-effects"));
        Assert.NotEmpty(CustomizationSectionCatalog.GetFeatureCards("launcher"));
        Assert.NotEmpty(CustomizationSectionCatalog.GetFeatureCards("layouts-tiling"));
        Assert.NotEmpty(CustomizationSectionCatalog.GetFeatureCards("advanced-risk-lab"));
    }

    [Fact]
    public void PresetCatalogUsesValidHexColorsAndStableKeys()
    {
        var presets = CustomizationPresetCatalog.DefaultPresets;
        var keys = presets.Select(preset => preset.Key).ToArray();

        Assert.All(presets, preset => Assert.True(HexColorValidator.IsValidHexColor(preset.AccentColorHex)));
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData("#4CC2FF", true)]
    [InlineData("#ff4fd8", true)]
    [InlineData("4CC2FF", false)]
    [InlineData("#XYZ123", false)]
    [InlineData("#1234567", false)]
    public void HexColorValidatorChecksExpectedFormat(string value, bool expected)
    {
        Assert.Equal(expected, HexColorValidator.IsValidHexColor(value));
    }

    [Fact]
    public void PersonalizationOperationResultFactoriesSetExpectedState()
    {
        var success = PersonalizationOperationResult.Succeeded("Done.");
        var failure = PersonalizationOperationResult.Failed("Coming later.", "App-only", "Preview support");

        Assert.True(success.Success);
        Assert.Equal("Low", success.RiskLevel);
        Assert.False(failure.Success);
        Assert.Equal("App-only", failure.RiskLevel);
        Assert.Equal("Preview support", failure.Requires);
    }
}
