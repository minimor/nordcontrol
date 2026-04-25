namespace NordControl.Core.Models;

public sealed record AppModule(
    string Key,
    string DisplayName,
    string Description,
    string Status);
