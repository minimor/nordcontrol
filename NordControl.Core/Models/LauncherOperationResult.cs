namespace NordControl.Core.Models;

public sealed record LauncherOperationResult(
    bool Success,
    string Message,
    string? Details = null)
{
    public static LauncherOperationResult Succeeded(string message, string? details = null)
    {
        return new LauncherOperationResult(true, message, details);
    }

    public static LauncherOperationResult Failed(string message, string? details = null)
    {
        return new LauncherOperationResult(false, message, details);
    }
}
