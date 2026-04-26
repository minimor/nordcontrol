using NordControl.Core.Models;

namespace NordControl.Core.Modules;

public static class LauncherSearchEngine
{
    public static IReadOnlyList<LauncherSearchResult> Search(
        IEnumerable<LauncherCommand> commands,
        string? query,
        int maxResults)
    {
        var terms = SplitTerms(query);
        var results = commands
            .Where(command => command.IsEnabled)
            .Select(command => Score(command, terms))
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Command.Title, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(maxResults, 1, 50))
            .ToList();

        return results;
    }

    private static LauncherSearchResult Score(LauncherCommand command, IReadOnlyList<string> terms)
    {
        if (terms.Count == 0)
        {
            return new LauncherSearchResult(command, 1, []);
        }

        var searchable = $"{command.Title} {command.Subtitle} {command.Category} {command.Keywords}".ToLowerInvariant();
        var title = command.Title.ToLowerInvariant();
        var category = command.Category.ToLowerInvariant();
        var matched = new List<string>();
        var score = 0;

        foreach (var term in terms)
        {
            if (!searchable.Contains(term, StringComparison.Ordinal))
            {
                continue;
            }

            matched.Add(term);
            score += 10;

            if (title.StartsWith(term, StringComparison.Ordinal))
            {
                score += 35;
            }
            else if (title.EndsWith(term, StringComparison.Ordinal))
            {
                score += 30;
            }
            else if (title.Contains(term, StringComparison.Ordinal))
            {
                score += 18;
            }

            if (category.Contains(term, StringComparison.Ordinal))
            {
                score += 8;
            }
        }

        if (matched.Count != terms.Count)
        {
            return new LauncherSearchResult(command, 0, matched);
        }

        return new LauncherSearchResult(command, score, matched);
    }

    private static IReadOnlyList<string> SplitTerms(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(term => term.ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
