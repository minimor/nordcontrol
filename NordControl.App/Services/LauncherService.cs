using System.Diagnostics;
using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Core.Services;

namespace NordControl.App.Services;

public sealed class LauncherService : ILauncherService
{
    private readonly IAppStateService appStateService;
    private readonly IDesktopWidgetService desktopWidgetService;
    private readonly IThemePackageService themePackageService;

    public LauncherService(
        IAppStateService appStateService,
        IDesktopWidgetService desktopWidgetService,
        IThemePackageService themePackageService)
    {
        this.appStateService = appStateService;
        this.desktopWidgetService = desktopWidgetService;
        this.themePackageService = themePackageService;
    }

    public event Action<string>? NavigationRequested;

    public event Action<string>? CustomizationSectionRequested;

    public event Action? WindowRefreshRequested;

    public string LastStatusMessage { get; private set; } = "Launcher ready.";

    public bool CloseAfterAction => appStateService.Settings.Launcher.CloseAfterAction;

    public IReadOnlyList<LauncherCommand> GetCommands()
    {
        var settings = appStateService.Settings.Launcher;
        var commands = new List<LauncherCommand>();

        if (settings.IncludeNordControlCommands)
        {
            commands.AddRange(LauncherCommandCatalog.BuiltInCommands.Where(command =>
                command.Category is not "System" || settings.IncludeSystemActions));
        }

        if (settings.IncludeApps)
        {
            commands.AddRange(GetAppCommands());
        }

        return commands;
    }

    public IReadOnlyList<LauncherSearchResult> Search(string query)
    {
        var settings = appStateService.Settings.Launcher;
        settings.Normalize();

        if (!settings.EnableLauncher)
        {
            return [];
        }

        return LauncherSearchEngine.Search(GetCommands(), query, settings.MaxResults);
    }

    public LauncherOperationResult Execute(LauncherCommand command)
    {
        try
        {
            var result = command.ActionType switch
            {
                "navigate" => Navigate(command.ActionPayload),
                "customization-section" => OpenCustomizationSection(command.ActionPayload),
                "window-manager" => ExecuteWindowManager(command.ActionPayload),
                "widgets" => ExecuteWidgetCommand(command.ActionPayload),
                "theme" => ApplyTheme(command.ActionPayload),
                "system" => ExecuteSystemCommand(command.ActionPayload),
                "app" => LaunchApp(command.ActionPayload),
                _ => LauncherOperationResult.Failed("Command is not implemented yet.", command.ActionType)
            };

            LastStatusMessage = result.Message;
            return result;
        }
        catch (Exception ex)
        {
            LastStatusMessage = $"Launcher command failed: {ex.Message}";
            return LauncherOperationResult.Failed("Launcher command failed.", ex.Message);
        }
    }

    public LauncherOperationResult RegisterHotkey()
    {
        LastStatusMessage = "Global hotkey registration is planned; Ctrl+Space works while NordControl is focused.";
        return LauncherOperationResult.Failed(
            "Native global hotkey registration is not enabled in V1.",
            "Use the shell Launcher button or Ctrl+Space while NordControl is focused.");
    }

    public LauncherOperationResult UnregisterHotkey()
    {
        return LauncherOperationResult.Succeeded("Launcher hotkey unregistered.");
    }

    private LauncherOperationResult Navigate(string moduleKey)
    {
        NavigationRequested?.Invoke(moduleKey);
        return LauncherOperationResult.Succeeded($"Opened {moduleKey}.");
    }

    private LauncherOperationResult OpenCustomizationSection(string sectionKey)
    {
        NavigationRequested?.Invoke("customization");
        CustomizationSectionRequested?.Invoke(sectionKey);
        return LauncherOperationResult.Succeeded($"Opened Customization -> {sectionKey}.");
    }

    private LauncherOperationResult ExecuteWindowManager(string payload)
    {
        NavigationRequested?.Invoke("window-manager");
        if (payload == "refresh")
        {
            WindowRefreshRequested?.Invoke();
            return LauncherOperationResult.Succeeded("Window Manager refresh requested.");
        }

        return LauncherOperationResult.Succeeded("Opened Window Manager.");
    }

    private LauncherOperationResult ExecuteWidgetCommand(string payload)
    {
        var settings = appStateService.Settings.Customization.DesktopWidgets;

        return payload switch
        {
            "show" => ShowWidgets(settings),
            "hide" => desktopWidgetService.HideWidgets().ToLauncherResult(),
            "toggle" => ToggleWidgets(settings),
            "reset-layout" => desktopWidgetService.ResetWidgetLayout().ToLauncherResult(),
            "lock" => SetWidgetLock(true),
            "unlock" => SetWidgetLock(false),
            _ => LauncherOperationResult.Failed("Unknown widget command.", payload)
        };
    }

    private LauncherOperationResult ShowWidgets(DesktopWidgetSettings settings)
    {
        settings.EnableWidgets = true;
        appStateService.Save();
        return desktopWidgetService.ShowWidgets().ToLauncherResult();
    }

    private LauncherOperationResult ToggleWidgets(DesktopWidgetSettings settings)
    {
        settings.EnableWidgets = true;
        appStateService.Save();
        return desktopWidgetService.ToggleWidgets().ToLauncherResult();
    }

    private LauncherOperationResult SetWidgetLock(bool locked)
    {
        appStateService.Settings.Customization.DesktopWidgets.LockWidgetPositions = locked;
        appStateService.Save();
        return LauncherOperationResult.Succeeded(locked ? "Widget positions locked." : "Widget positions unlocked.");
    }

    private LauncherOperationResult ApplyTheme(string themeKey)
    {
        var theme = themePackageService.GetBuiltInPackages()
            .FirstOrDefault(package => string.Equals(package.Key, themeKey, StringComparison.Ordinal));
        if (theme is null)
        {
            return LauncherOperationResult.Failed("Theme package was not found.", themeKey);
        }

        appStateService.Settings.Customization.SelectedThemePackageKey = theme.Key;
        appStateService.Settings.Customization.NordControlAccentColorHex = theme.AccentColorHex;
        appStateService.Settings.Customization.EnableGlassStyleInApp = theme.EnableGlass;
        appStateService.Save();
        return LauncherOperationResult.Succeeded($"{theme.Name} theme selected.");
    }

    private LauncherOperationResult ExecuteSystemCommand(string payload)
    {
        return payload switch
        {
            "appdata" => OpenFolder(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NordControl")),
            "settings-folder" => OpenFolder(Path.GetDirectoryName(appStateService.SettingsFilePath) ?? string.Empty),
            "copy-settings-path" => LauncherOperationResult.Failed(
                "Copying to clipboard is planned for the launcher overlay.",
                appStateService.SettingsFilePath),
            "github" => OpenUrl("https://github.com/minimor/nordcontrol"),
            _ => LauncherOperationResult.Failed("Unknown system command.", payload)
        };
    }

    private static LauncherOperationResult OpenFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return LauncherOperationResult.Failed("Folder path is unavailable.");
        }

        Directory.CreateDirectory(folderPath);
        Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true });
        return LauncherOperationResult.Succeeded($"Opened {folderPath}.");
    }

    private static LauncherOperationResult OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        return LauncherOperationResult.Succeeded("Opened GitHub repository.");
    }

    private static LauncherOperationResult LaunchApp(string shortcutPath)
    {
        if (!File.Exists(shortcutPath))
        {
            return LauncherOperationResult.Failed("App shortcut was not found.", shortcutPath);
        }

        Process.Start(new ProcessStartInfo(shortcutPath) { UseShellExecute = true });
        return LauncherOperationResult.Succeeded($"Launched {Path.GetFileNameWithoutExtension(shortcutPath)}.");
    }

    private static IReadOnlyList<LauncherCommand> GetAppCommands()
    {
        var roots = new[]
        {
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                "Programs"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programs")
        };

        return roots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.lnk", SearchOption.AllDirectories))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(400)
            .Select(shortcut => new LauncherCommand
            {
                Id = $"app:{shortcut}",
                Title = Path.GetFileNameWithoutExtension(shortcut),
                Subtitle = shortcut,
                Category = "App",
                IconHint = "A",
                Keywords = Path.GetFileNameWithoutExtension(shortcut),
                RiskLevel = "Low",
                ActionType = "app",
                ActionPayload = shortcut,
                IsEnabled = true
            })
            .ToList();
    }
}

internal static class WidgetOperationResultExtensions
{
    public static LauncherOperationResult ToLauncherResult(this WidgetOperationResult result)
    {
        return result.Success
            ? LauncherOperationResult.Succeeded(result.Message, result.Details)
            : LauncherOperationResult.Failed(result.Message, result.Details);
    }
}
