namespace NordControl.Core.Models;

public sealed record LauncherSearchResult(
    LauncherCommand Command,
    int Score,
    IReadOnlyList<string> MatchedTerms);
