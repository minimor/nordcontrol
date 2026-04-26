using NordControl.Core.Models;

namespace NordControl.Core.Modules;

public static class LauncherCommandCatalog
{
    public static IReadOnlyList<LauncherCommand> BuiltInCommands { get; } =
    [
        Navigation("open-dashboard", "Open Dashboard", "Go to the NordControl overview.", "dashboard", "dashboard home overview"),
        Navigation("open-window-manager", "Open Window Manager", "Manage open windows and topmost pins.", "window-manager", "windows pin topmost refresh"),
        Navigation("open-customization", "Open Customization", "Open the Customization Studio.", "customization", "customize studio appearance"),
        Navigation("open-settings", "Open Settings", "Edit NordControl preferences.", "settings", "preferences config options"),
        CustomizationSection("open-taskbar-lab", "Open Taskbar Lab", "Jump to Customization -> Taskbar.", "taskbar", "taskbar lab"),
        CustomizationSection("open-desktop-widgets", "Open Desktop Widgets", "Jump to Customization -> Desktop Widgets.", "desktop-widgets", "widgets overlay"),
        CustomizationSection("open-themes", "Open Themes", "Jump to Customization -> Themes.", "themes", "theme package editor"),
        Widget("show-widgets", "Show Widgets", "Show enabled desktop widget overlays.", "show", "widgets overlay desktop"),
        Widget("hide-widgets", "Hide Widgets", "Hide all desktop widget overlays.", "hide", "widgets overlay close"),
        Widget("toggle-widgets", "Toggle Widgets", "Show or hide desktop widget overlays.", "toggle", "widgets overlay toggle"),
        Widget("reset-widget-layout", "Reset Widget Layout", "Return widgets to default positions.", "reset-layout", "widgets layout reset"),
        Widget("lock-widget-positions", "Lock Widget Positions", "Disable widget dragging and resizing.", "lock", "widgets lock positions"),
        Widget("unlock-widget-positions", "Unlock Widget Positions", "Enable widget dragging and resizing.", "unlock", "widgets unlock positions"),
        new()
        {
            Id = "refresh-windows",
            Title = "Refresh Windows",
            Subtitle = "Refresh the Window Manager list.",
            Category = "Window Manager",
            IconHint = "R",
            Keywords = "window manager refresh reload",
            RiskLevel = "Low",
            ActionType = "window-manager",
            ActionPayload = "refresh"
        },
        CustomizationSection("open-theme-editor", "Open Theme Editor", "Open the theme package and custom editor area.", "themes", "theme editor package"),
        Theme("apply-fluent-dark", "Apply Fluent Dark Theme", "Apply the built-in Fluent Dark theme package.", "fluent-dark", "theme fluent dark"),
        Theme("apply-glass-blue", "Apply Glass Blue Theme", "Apply the built-in Glass Blue theme package.", "glass-blue", "theme glass blue"),
        System("open-appdata", "Open NordControl AppData", "Open the NordControl AppData folder.", "appdata", "folder files data"),
        System("open-settings-folder", "Open Settings Folder", "Open the folder containing settings.json.", "settings-folder", "settings json folder"),
        System("copy-settings-path", "Copy Settings Path", "Copy the settings.json path to clipboard when available.", "copy-settings-path", "settings path copy"),
        System("open-github", "Open GitHub Repository", "Open minimor/nordcontrol in the browser.", "github", "repo source browser")
    ];

    private static LauncherCommand Navigation(string id, string title, string subtitle, string moduleKey, string keywords)
    {
        return new LauncherCommand
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            Category = "Navigation",
            IconHint = "N",
            Keywords = keywords,
            RiskLevel = "Low",
            ActionType = "navigate",
            ActionPayload = moduleKey
        };
    }

    private static LauncherCommand CustomizationSection(string id, string title, string subtitle, string sectionKey, string keywords)
    {
        return new LauncherCommand
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            Category = "Customization",
            IconHint = "C",
            Keywords = keywords,
            RiskLevel = "Low",
            ActionType = "customization-section",
            ActionPayload = sectionKey
        };
    }

    private static LauncherCommand Widget(string id, string title, string subtitle, string payload, string keywords)
    {
        return new LauncherCommand
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            Category = "Widgets",
            IconHint = "W",
            Keywords = keywords,
            RiskLevel = "Low",
            ActionType = "widgets",
            ActionPayload = payload
        };
    }

    private static LauncherCommand Theme(string id, string title, string subtitle, string themeKey, string keywords)
    {
        return new LauncherCommand
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            Category = "Themes",
            IconHint = "T",
            Keywords = keywords,
            RiskLevel = "Low",
            ActionType = "theme",
            ActionPayload = themeKey
        };
    }

    private static LauncherCommand System(string id, string title, string subtitle, string payload, string keywords)
    {
        return new LauncherCommand
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            Category = "System",
            IconHint = "S",
            Keywords = keywords,
            RiskLevel = "Low",
            ActionType = "system",
            ActionPayload = payload
        };
    }
}
