using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Windows.Services;

namespace NordControl.Tests;

public sealed class TaskbarLabTests
{
    [Fact]
    public void TaskbarPresetCatalogContainsRequiredPresets()
    {
        var names = TaskbarPresetCatalog.Presets
            .Select(preset => preset.Name)
            .ToArray();

        Assert.Contains("Fluent Transparent", names);
        Assert.Contains("Glass Floating", names);
        Assert.Contains("Compact Focus", names);
        Assert.Contains("Productivity Bar", names);
        Assert.Contains("Cyber Neon", names);
        Assert.Contains("Minimal Dark", names);
    }

    [Fact]
    public void TaskbarPresetCatalogUsesStableKeysAndValidAccentColors()
    {
        var presets = TaskbarPresetCatalog.Presets;
        var keys = presets.Select(preset => preset.Key).ToArray();

        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.All(presets, preset => Assert.True(HexColorValidator.IsValidHexColor(preset.AccentColorHex)));
    }

    [Fact]
    public void TaskbarSettingsNormalizeInvalidPresetAndMediumRiskGate()
    {
        var settings = new TaskbarSettings
        {
            SelectedTaskbarPresetKey = "missing",
            EnableTaskbarLab = false,
            AllowMediumRiskTaskbarChanges = true,
            UsePreviewOnlyMode = false,
            ShowTaskbarWarnings = false
        };

        settings.Normalize();

        Assert.Equal(TaskbarPresetCatalog.DefaultPresetKey, settings.SelectedTaskbarPresetKey);
        Assert.False(settings.AllowMediumRiskTaskbarChanges);
        Assert.False(settings.UsePreviewOnlyMode);
        Assert.False(settings.ShowTaskbarWarnings);
    }

    [Fact]
    public void TaskbarOperationResultFactoriesSetExpectedState()
    {
        var success = TaskbarOperationResult.Succeeded("Preview loaded.", "Preview-only");
        var failure = TaskbarOperationResult.Failed("Coming later.", "Risk Lab future", "No shell changes.");

        Assert.True(success.Success);
        Assert.Equal("Preview-only", success.RiskLevel);
        Assert.False(failure.Success);
        Assert.Equal("Risk Lab future", failure.RiskLevel);
        Assert.Equal("No shell changes.", failure.Details);
    }

    [Fact]
    public void ApplyPreviewOnlyPresetReturnsFriendlyResult()
    {
        var service = new WindowsTaskbarService();
        var result = service.ApplyPreset(TaskbarPresetCatalog.DefaultPresetKey, allowMediumRisk: false);

        Assert.False(result.Success);
        Assert.Contains("preview-only", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No Explorer", result.Details, StringComparison.OrdinalIgnoreCase);
    }
}
