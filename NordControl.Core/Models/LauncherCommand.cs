namespace NordControl.Core.Models;

public sealed class LauncherCommand
{
    public string Id { get; set; } = "command";

    public string Title { get; set; } = "Command";

    public string Subtitle { get; set; } = string.Empty;

    public string Category { get; set; } = "NordControl";

    public string IconHint { get; set; } = ">";

    public string Keywords { get; set; } = string.Empty;

    public string RiskLevel { get; set; } = "Low";

    public bool IsEnabled { get; set; } = true;

    public string ActionType { get; set; } = "none";

    public string ActionPayload { get; set; } = string.Empty;
}
