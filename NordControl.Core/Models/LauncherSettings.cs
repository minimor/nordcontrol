namespace NordControl.Core.Models;

public sealed class LauncherSettings
{
    public const string DefaultHotkeyGesture = "Ctrl+Space";

    public bool EnableLauncher { get; set; } = true;

    public bool StartWithApp { get; set; } = true;

    public string HotkeyGesture { get; set; } = DefaultHotkeyGesture;

    public bool ShowOnStartup { get; set; }

    public bool IncludeNordControlCommands { get; set; } = true;

    public bool IncludeApps { get; set; } = true;

    public bool IncludeSystemActions { get; set; } = true;

    public bool CloseAfterAction { get; set; } = true;

    public int MaxResults { get; set; } = 10;

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(HotkeyGesture) || !IsSupportedHotkey(HotkeyGesture))
        {
            HotkeyGesture = DefaultHotkeyGesture;
        }

        MaxResults = Math.Clamp(MaxResults, 8, 20);
    }

    private static bool IsSupportedHotkey(string gesture)
    {
        var normalized = gesture.Trim();
        return normalized.Equals("Ctrl+Space", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("Ctrl+Alt+Space", StringComparison.OrdinalIgnoreCase);
    }
}
