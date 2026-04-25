namespace NordControl.Core.Models;

public sealed class ThemePackageOperationResult
{
    private ThemePackageOperationResult(bool success, string message, string? filePath)
    {
        Success = success;
        Message = message;
        FilePath = filePath;
    }

    public bool Success { get; }

    public string Message { get; }

    public string? FilePath { get; }

    public static ThemePackageOperationResult Succeeded(string message, string? filePath = null)
    {
        return new ThemePackageOperationResult(true, message, filePath);
    }

    public static ThemePackageOperationResult Failed(string message, string? filePath = null)
    {
        return new ThemePackageOperationResult(false, message, filePath);
    }
}
