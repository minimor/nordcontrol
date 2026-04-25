using NordControl.Core.Models;

namespace NordControl.Core.Windows;

public static class WindowInfoFilter
{
    public static IReadOnlyList<WindowInfo> Apply(IEnumerable<WindowInfo> windows, string? searchText)
    {
        var windowList = windows.ToList();
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return windowList;
        }

        var normalizedSearch = searchText.Trim();

        return windowList
            .Where(window =>
                window.Title.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                window.ProcessName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
