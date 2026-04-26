namespace NordControl.Core.Models;

public sealed class DesktopWidgetDefinition
{
    public string Key { get; set; } = "clock";

    public string Name { get; set; } = "Clock";

    public string Description { get; set; } = "A lightweight desktop clock with date.";

    public string Category { get; set; } = "Essentials";

    public string RiskLevel { get; set; } = "Low";

    public bool IsImplemented { get; set; } = true;

    public string AccentColorHex { get; set; } = "#4CC2FF";
}
