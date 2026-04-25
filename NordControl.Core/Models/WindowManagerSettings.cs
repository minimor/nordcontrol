namespace NordControl.Core.Models;

public sealed class WindowManagerSettings
{
    public bool RefreshOnStartup { get; set; } = true;

    public int AutoRefreshIntervalSeconds { get; set; }

    public bool ConfirmBeforePinning { get; set; }

    public bool ShowUnknownProcesses { get; set; } = true;

    public void Normalize()
    {
        if (AutoRefreshIntervalSeconds < 0)
        {
            AutoRefreshIntervalSeconds = 0;
        }
    }
}
