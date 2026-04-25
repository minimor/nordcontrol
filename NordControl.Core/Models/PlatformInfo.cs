namespace NordControl.Core.Models;

public sealed record PlatformInfo(
    string OperatingSystem,
    string Runtime,
    string Architecture,
    bool IsWindows);
