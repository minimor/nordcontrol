namespace NordControl.Core.Models;

public sealed record TaskbarState(
    bool IsWindows,
    string TaskbarAlignment,
    string AutoHideEnabled,
    string SmallTaskbarButtons,
    string TransparencyMode,
    string Notes,
    DateTime LastLoadedAt);
