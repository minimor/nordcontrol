namespace NordControl.Core.Models;

public sealed record PersonalizationOperationResult(
    bool Success,
    string Message,
    string? Requires,
    string RiskLevel,
    string? Details = null)
{
    public static PersonalizationOperationResult Succeeded(
        string message,
        string riskLevel = "Low",
        string? details = null)
    {
        return new PersonalizationOperationResult(true, message, null, riskLevel, details);
    }

    public static PersonalizationOperationResult Failed(
        string message,
        string riskLevel = "Low",
        string? requires = null,
        string? details = null)
    {
        return new PersonalizationOperationResult(false, message, requires, riskLevel, details);
    }
}
