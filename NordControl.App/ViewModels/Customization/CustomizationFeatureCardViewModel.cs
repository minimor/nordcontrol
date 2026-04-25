using NordControl.Core.Models;

namespace NordControl.App.ViewModels.Customization;

public sealed class CustomizationFeatureCardViewModel(CustomizationFeatureCard card)
{
    public string Name { get; } = card.Name;

    public string Description { get; } = card.Description;

    public string Badge { get; } = card.Badge;

    public string RiskLevel { get; } = card.RiskLevel;

    public string AccentColorHex { get; } = card.AccentColorHex;
}
