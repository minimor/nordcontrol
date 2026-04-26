namespace NordControl.Core.Models;

public sealed class CustomizationSettings
{
    public string LastSelectedSectionKey { get; set; } = "overview";

    public string SelectedPresetKey { get; set; } = "fluent-dark";

    public string SelectedThemePackageKey { get; set; } = "fluent-dark";

    public string LastExportedThemePath { get; set; } = string.Empty;

    public string LastImportedThemePath { get; set; } = string.Empty;

    public bool ApplyThemeToNordControlShell { get; set; } = true;

    public string NordControlAccentColorHex { get; set; } = "#4CC2FF";

    public bool EnableGlassStyleInApp { get; set; } = true;

    public bool AllowLowRiskWindowsPersonalization { get; set; } = true;

    public void Normalize()
    {
        if (!NordControl.Core.Modules.CustomizationSectionCatalog.IsKnownSectionKey(LastSelectedSectionKey))
        {
            LastSelectedSectionKey = NordControl.Core.Modules.CustomizationSectionCatalog.DefaultSectionKey;
        }

        if (string.IsNullOrWhiteSpace(SelectedPresetKey))
        {
            SelectedPresetKey = "fluent-dark";
        }

        if (string.IsNullOrWhiteSpace(SelectedThemePackageKey))
        {
            SelectedThemePackageKey = NordControl.Core.Modules.ThemePackageCatalog.DefaultThemePackageKey;
        }

        LastExportedThemePath ??= string.Empty;
        LastImportedThemePath ??= string.Empty;

        if (!HexColorValidator.IsValidHexColor(NordControlAccentColorHex))
        {
            NordControlAccentColorHex = "#4CC2FF";
        }
    }
}
