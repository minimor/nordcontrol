namespace NordControl.Core.Models;

public sealed record CustomizationPreset(
    string Key,
    string Name,
    string Description,
    string AccentColorHex,
    string BackgroundHint,
    string StyleMood,
    bool AppliesToNordControlOnly,
    string RiskLevel);
