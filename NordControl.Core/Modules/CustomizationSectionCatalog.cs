using NordControl.Core.Models;

namespace NordControl.Core.Modules;

public static class CustomizationSectionCatalog
{
    public const string DefaultSectionKey = "overview";

    public static IReadOnlyList<CustomizationSection> Sections { get; } =
    [
        new("overview", "Overview", "Snapshot, safety status, and the desktop environment roadmap.", "Safe Layer", "#4CC2FF"),
        new("themes", "Themes", "NordControl presets and low-risk Windows theme controls.", "Presets", "#8CF5D2"),
        new("taskbar", "Taskbar", "Taskbar Lab previews and safe read-only Windows taskbar state.", "Taskbar Lab", "#72D7FF"),
        new("start-menu", "Start Menu", "Launcher and Start menu layout experiments for later stages.", "Planned", "#FFD166"),
        new("desktop-widgets", "Desktop Widgets", "A future widget layer for glanceable desktop modules.", "Planned", "#A78BFA"),
        new("window-effects", "Window Effects", "Visual window effects, overlays, and per-app polish ideas.", "Planned", "#FF8FB3"),
        new("launcher", "Launcher", "A command-first launcher inspired by fast desktop workflows.", "Command Lab", "#FF4FD8"),
        new("layouts-tiling", "Layouts / Tiling", "Window layout, zones, and workspace concepts.", "Layout Lab", "#8FB7FF"),
        new("advanced-risk-lab", "Advanced / Risk Lab", "Clearly separated experiments that may break after Windows updates.", "Experimental", "#FF5C5C", true)
    ];

    public static IReadOnlyList<CustomizationFeatureCard> FeatureCards { get; } =
    [
        new("overview", "Desktop Environment Lab", "A long-term direction for making Windows feel personal, modular, and delightful.", "Roadmap", "Safe", "#4CC2FF"),
        new("overview", "Safe Layer", "Current-user personalization and app-only previews stay separated from risky shell work.", "Active", "Low", "#8CF5D2"),
        new("overview", "Presets", "NordControl visual presets are stored safely and can evolve into richer theme packages.", "Active", "App-only", "#FFD166"),
        new("overview", "Risk Lab", "Advanced shell experiments are isolated behind explicit warnings and future confirmations.", "Separated", "High", "#FF5C5C"),

        new("taskbar", "Transparent Taskbar", "A future taskbar appearance mode with transparency controls.", "Planned", "Risk Lab", "#72D7FF"),
        new("taskbar", "Blur / Acrylic Taskbar", "A soft glass taskbar concept with blur and color tuning.", "Planned", "Risk Lab", "#8CF5D2"),
        new("taskbar", "Compact Taskbar", "Denser taskbar spacing and a cleaner utility layout.", "Planned", "Experimental", "#A7B2C2"),
        new("taskbar", "Centered / Floating Taskbar", "A floating taskbar concept inspired by modern desktop shells.", "Concept", "Experimental", "#A78BFA"),
        new("taskbar", "Taskbar Modules", "Small widgets and modules near taskbar surfaces.", "Coming later", "Risk Lab", "#FF8FB3"),

        new("start-menu", "Compact Launcher", "A cleaner launch surface focused on speed and muscle memory.", "Concept", "Experimental", "#FFD166"),
        new("start-menu", "App Grid", "A curated grid for frequently used tools and folders.", "Planned", "Safe preview", "#8FB7FF"),
        new("start-menu", "Quick Folders", "Pinned workspaces, folders, and project shortcuts.", "Planned", "Safe preview", "#8CF5D2"),
        new("start-menu", "Power Shortcuts", "Sleep, lock, restart, and session actions with confirmation.", "Planned", "Medium", "#FF8FB3"),
        new("start-menu", "Search-First Layout", "A Start experience that begins with search and commands.", "Concept", "Experimental", "#FF4FD8"),

        new("desktop-widgets", "Clock / Weather", "A future desktop card for time and weather at a glance.", "Placeholder", "Safe preview", "#72D7FF"),
        new("desktop-widgets", "System Monitor", "CPU, RAM, disk, and power widgets for the desktop.", "Planned", "Safe preview", "#8CF5D2"),
        new("desktop-widgets", "Music Controls", "Media controls and current track display.", "Planned", "Safe preview", "#A78BFA"),
        new("desktop-widgets", "Quick Notes", "A small personal notes panel for desktop workflows.", "Concept", "Safe preview", "#FFD166"),
        new("desktop-widgets", "Shortcuts Panel", "A visual shortcuts strip for apps and folders.", "Concept", "Safe preview", "#FF8FB3"),

        new("window-effects", "Focus Overlay", "A subtle rounded highlight for the active window.", "Concept", "Experimental", "#8CF5D2"),
        new("window-effects", "Per-App Opacity", "Per-app transparency rules for selected windows.", "Planned", "Risk Lab", "#72D7FF"),
        new("window-effects", "Always-On-Top Rules", "Future rules that build on the current Window Manager pinning.", "Planned", "Medium", "#FFD166"),
        new("window-effects", "Window Border Accent", "A tasteful accent indicator around important windows.", "Concept", "Experimental", "#FF4FD8"),
        new("window-effects", "Snap / Tiling Helpers", "Visual helpers for snapping and arranging windows.", "Planned", "Medium", "#8FB7FF"),

        new("launcher", "App Search", "Fast app lookup from a command-first launcher.", "Planned", "Safe preview", "#FF4FD8"),
        new("launcher", "Command Palette", "A keyboard-first command surface for NordControl actions.", "Concept", "Safe preview", "#8CF5D2"),
        new("launcher", "Quick Actions", "Small scripts and safe shortcuts launched from one place.", "Planned", "Medium", "#FFD166"),
        new("launcher", "Run Commands", "A careful command runner with history and confirmation.", "Planned", "Medium", "#72D7FF"),
        new("launcher", "File Shortcuts", "Pinned files, folders, and workspace shortcuts.", "Planned", "Safe preview", "#A78BFA"),

        new("layouts-tiling", "Auto Tiling", "Future opt-in window tiling with clear escape hatches.", "Concept", "Experimental", "#8FB7FF"),
        new("layouts-tiling", "Zones", "Named placement zones for repeated layouts.", "Planned", "Medium", "#8CF5D2"),
        new("layouts-tiling", "Save / Restore Layouts", "Capture and restore window arrangements.", "Planned", "Medium", "#FFD166"),
        new("layouts-tiling", "Per-Workspace Layout", "Different layouts for work, gaming, and focus modes.", "Concept", "Experimental", "#FF8FB3"),

        new("advanced-risk-lab", "Taskbar Replacement", "A full shell-level taskbar experiment reserved for explicit future opt-in.", "Disabled", "High", "#FF5C5C"),
        new("advanced-risk-lab", "Start Menu Replacement", "A future replacement concept that must remain isolated and reversible.", "Disabled", "High", "#FF8FB3"),
        new("advanced-risk-lab", "Explorer Integration", "Integration experiments that can break after Windows updates.", "Disabled", "High", "#FFD166"),
        new("advanced-risk-lab", "Custom Alt+Tab", "A custom switcher concept requiring careful input/window handling.", "Disabled", "High", "#A78BFA"),
        new("advanced-risk-lab", "Virtual Desktop Viewer", "A visual virtual desktop map for a later risk-reviewed stage.", "Disabled", "High", "#72D7FF")
    ];

    public static bool IsKnownSectionKey(string? key)
    {
        return Sections.Any(section => section.Key == key);
    }

    public static IReadOnlyList<CustomizationFeatureCard> GetFeatureCards(string sectionKey)
    {
        return FeatureCards
            .Where(card => card.SectionKey == sectionKey)
            .ToList();
    }
}
