namespace NordControl.Core.Models;

public sealed record WindowInfo(
    nint Handle,
    string Title,
    string ProcessName,
    int? ProcessId,
    bool IsTopMost,
    DateTime LastSeenAt);
