namespace NordControl.Core.Models;

public sealed record TaskbarOperationResult(
    bool Success,
    string Message,
    string RiskLevel,
    string? Details = null)
{
    public static TaskbarOperationResult Succeeded(
        string message,
        string riskLevel = "Preview",
        string? details = null)
    {
        return new TaskbarOperationResult(true, message, riskLevel, details);
    }

    public static TaskbarOperationResult Failed(
        string message,
        string riskLevel = "Preview",
        string? details = null)
    {
        return new TaskbarOperationResult(false, message, riskLevel, details);
    }
}
