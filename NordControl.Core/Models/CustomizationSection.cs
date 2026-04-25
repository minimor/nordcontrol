namespace NordControl.Core.Models;

public sealed record CustomizationSection(
    string Key,
    string Name,
    string Description,
    string Badge,
    string AccentColorHex,
    bool IsRiskLab = false);
