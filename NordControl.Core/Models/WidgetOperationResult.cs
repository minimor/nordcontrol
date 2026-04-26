namespace NordControl.Core.Models;

public sealed record WidgetOperationResult(
    bool Success,
    string Message,
    string? Details = null)
{
    public static WidgetOperationResult Succeeded(string message, string? details = null)
    {
        return new WidgetOperationResult(true, message, details);
    }

    public static WidgetOperationResult Failed(string message, string? details = null)
    {
        return new WidgetOperationResult(false, message, details);
    }
}
