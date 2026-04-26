namespace NordControl.Core.Models;

public sealed class TaskbarPreset
{
    public string Key { get; set; } = "fluent-transparent";

    public string Name { get; set; } = "Fluent Transparent";

    public string Description { get; set; } = "A calm translucent taskbar concept with soft Windows-style surfaces.";

    public string VisualStyle { get; set; } = "Translucent";

    public string RiskLevel { get; set; } = "Preview";

    public string AccentColorHex { get; set; } = "#4CC2FF";

    public bool IsPreviewOnly { get; set; } = true;

    public bool RequiresExplorerRestart { get; set; }

    public bool IsImplemented { get; set; }
}
