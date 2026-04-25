using NordControl.Core.Models;

namespace NordControl.Core.Modules;

public static class AppModuleCatalog
{
    public static IReadOnlyList<AppModule> DefaultModules { get; } =
    [
        new(
            "dashboard",
            "Dashboard",
            "A compact overview for system health, shortcuts, and current profile state.",
            "Shell placeholder"),
        new(
            "window-manager",
            "Window Manager",
            "Manage open windows and pin important windows above others.",
            "Active"),
        new(
            "performance-profiles",
            "Performance Profiles",
            "Personal presets for balancing speed, noise, power, and focus.",
            "Planned"),
        new(
            "customization",
            "Customization",
            "Make Windows feel more personal, modern and enjoyable.",
            "Studio"),
        new(
            "settings",
            "Settings",
            "App preferences, startup behavior, safety controls, and logs.",
            "Planned")
    ];
}
