using NordControl.Core.Models;
using NordControl.Core.Modules;

namespace NordControl.Tests;

public sealed class LauncherTests
{
    [Fact]
    public void LauncherSettingsNormalizeInvalidValues()
    {
        var settings = new LauncherSettings
        {
            HotkeyGesture = "bad-hotkey",
            MaxResults = 500
        };

        settings.Normalize();

        Assert.Equal(LauncherSettings.DefaultHotkeyGesture, settings.HotkeyGesture);
        Assert.Equal(20, settings.MaxResults);
    }

    [Fact]
    public void LauncherSettingsClampSmallMaxResults()
    {
        var settings = new LauncherSettings
        {
            MaxResults = 2
        };

        settings.Normalize();

        Assert.Equal(8, settings.MaxResults);
    }

    [Fact]
    public void CommandCatalogContainsRequiredCommands()
    {
        var ids = LauncherCommandCatalog.BuiltInCommands.Select(command => command.Id).ToArray();

        Assert.Contains("open-dashboard", ids);
        Assert.Contains("open-window-manager", ids);
        Assert.Contains("open-customization", ids);
        Assert.Contains("open-settings", ids);
        Assert.Contains("open-taskbar-lab", ids);
        Assert.Contains("open-desktop-widgets", ids);
        Assert.Contains("show-widgets", ids);
        Assert.Contains("hide-widgets", ids);
        Assert.Contains("reset-widget-layout", ids);
        Assert.Contains("apply-fluent-dark", ids);
        Assert.Contains("apply-glass-blue", ids);
        Assert.Contains("open-github", ids);
    }

    [Fact]
    public void SearchReturnsRelevantCommand()
    {
        var results = LauncherSearchEngine.Search(LauncherCommandCatalog.BuiltInCommands, "settings", 10);

        Assert.NotEmpty(results);
        Assert.Equal("open-settings", results[0].Command.Id);
    }

    [Fact]
    public void SearchRespectsMaxResults()
    {
        var commands = Enumerable.Range(0, 20)
            .Select(index => new LauncherCommand
            {
                Id = $"command-{index}",
                Title = $"Command {index}",
                Keywords = "common"
            });

        var results = LauncherSearchEngine.Search(commands, "common", 8);

        Assert.Equal(8, results.Count);
    }

    [Fact]
    public void SearchSkipsDisabledCommands()
    {
        var commands = new[]
        {
            new LauncherCommand { Id = "enabled", Title = "Enabled Command", Keywords = "target" },
            new LauncherCommand { Id = "disabled", Title = "Disabled Command", Keywords = "target", IsEnabled = false }
        };

        var results = LauncherSearchEngine.Search(commands, "target", 10);

        Assert.Single(results);
        Assert.Equal("enabled", results[0].Command.Id);
    }

    [Fact]
    public void CategoryFilteringCanExcludeSystemActions()
    {
        var settings = new LauncherSettings
        {
            IncludeNordControlCommands = true,
            IncludeSystemActions = false
        };

        var commands = LauncherCommandCatalog.BuiltInCommands
            .Where(command => command.Category is not "System" || settings.IncludeSystemActions)
            .ToList();

        Assert.DoesNotContain(commands, command => command.Category == "System");
        Assert.Contains(commands, command => command.Id == "open-dashboard");
    }
}
