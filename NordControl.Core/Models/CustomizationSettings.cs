namespace NordControl.Core.Models;

public sealed class CustomizationSettings
{
    public string LastSelectedSectionKey { get; set; } = "overview";

    public string SelectedPresetKey { get; set; } = "fluent-dark";

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

        if (!HexColorValidator.IsValidHexColor(NordControlAccentColorHex))
        {
            NordControlAccentColorHex = "#4CC2FF";
        }
    }
}
