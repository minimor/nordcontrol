namespace NordControl.Core.Models;

public sealed record CustomizationFeatureCard(
    string SectionKey,
    string Name,
    string Description,
    string Badge,
    string RiskLevel,
    string AccentColorHex);
