using NordControl.Core.Models;
using NordControl.Core.Services;

namespace NordControl.App.Services;

internal sealed class DesignTimeAppSettingsService : IAppSettingsService
{
    public string SettingsFilePath { get; } = @"%AppData%\NordControl\settings.json";

    public string LastStatusMessage { get; private set; } = "Design-time settings loaded.";

    public AppSettings Load()
    {
        LastStatusMessage = "Design-time settings loaded.";
        return AppSettings.CreateDefault();
    }

    public void Save(AppSettings settings)
    {
        LastStatusMessage = "Design-time settings saved.";
    }

    public AppSettings ResetToDefaults()
    {
        LastStatusMessage = "Design-time settings reset.";
        return AppSettings.CreateDefault();
    }
}

internal sealed class DesignTimePlatformInfoService : IPlatformInfoService
{
    public PlatformInfo GetPlatformInfo()
    {
        return new PlatformInfo("Windows", ".NET", "x64", true);
    }
}

internal sealed class DesignTimeWindowManagerService : IWindowManagerService
{
    public IReadOnlyList<WindowInfo> GetOpenWindows()
    {
        return
        [
            new WindowInfo(0x10001, "Untitled - Notepad", "notepad", 1200, false, DateTime.Now),
            new WindowInfo(0x10002, "NordControl", "NordControl.App", 2400, true, DateTime.Now)
        ];
    }

    public WindowOperationResult SetTopMost(nint hwnd)
    {
        return WindowOperationResult.Succeeded("Window pinned as topmost.", hwnd);
    }

    public WindowOperationResult RemoveTopMost(nint hwnd)
    {
        return WindowOperationResult.Succeeded("Window returned to normal stacking.", hwnd);
    }

    public bool IsTopMost(nint hwnd)
    {
        return hwnd == 0x10002;
    }
}

internal sealed class DesignTimeWindowsPersonalizationService : IWindowsPersonalizationService
{
    public WindowsPersonalizationState GetCurrentState()
    {
        return new WindowsPersonalizationState(
            "Dark",
            "Dark",
            true,
            "#4CC2FF",
            false,
            @"C:\Windows\Web\Wallpaper\Windows\img0.jpg",
            DateTime.Now);
    }

    public PersonalizationOperationResult SetAppsTheme(string theme)
    {
        return PersonalizationOperationResult.Succeeded("Design-time apps theme updated.");
    }

    public PersonalizationOperationResult SetSystemTheme(string theme)
    {
        return PersonalizationOperationResult.Succeeded("Design-time system theme updated.");
    }

    public PersonalizationOperationResult SetTransparencyEffects(bool enabled)
    {
        return PersonalizationOperationResult.Succeeded("Design-time transparency updated.");
    }

    public PersonalizationOperationResult SetAccentColor(string hexColor)
    {
        return PersonalizationOperationResult.Failed("Design-time accent color is preview-only.", "App-only");
    }

    public PersonalizationOperationResult SetAccentColorOnTitleBars(bool enabled)
    {
        return PersonalizationOperationResult.Succeeded("Design-time title bar accent updated.");
    }

    public PersonalizationOperationResult SetWallpaper(string filePath)
    {
        return PersonalizationOperationResult.Failed("Design-time wallpaper is preview-only.", "App-only");
    }
}

internal sealed class DesignTimeTaskbarService : ITaskbarService
{
    public TaskbarState GetCurrentState()
    {
        return new TaskbarState(
            true,
            "Center",
            "Disabled",
            "Unknown",
            "Transparency effects enabled",
            "Design-time read-only snapshot.",
            DateTime.Now);
    }

    public IReadOnlyList<TaskbarPreset> GetPresets()
    {
        return NordControl.Core.Modules.TaskbarPresetCatalog.Presets;
    }

    public TaskbarOperationResult PreviewPreset(string presetKey)
    {
        var preset = NordControl.Core.Modules.TaskbarPresetCatalog.GetPresetOrDefault(presetKey);
        return TaskbarOperationResult.Succeeded($"{preset.Name} loaded in the design-time preview.", preset.RiskLevel);
    }

    public TaskbarOperationResult ApplyPreset(string presetKey, bool allowMediumRisk)
    {
        var preset = NordControl.Core.Modules.TaskbarPresetCatalog.GetPresetOrDefault(presetKey);
        return TaskbarOperationResult.Failed(
            $"{preset.Name} is preview-only in Taskbar Lab V1.",
            preset.RiskLevel,
            "Design-time service does not change Windows.");
    }

    public TaskbarOperationResult ResetPreview()
    {
        return TaskbarOperationResult.Succeeded("Design-time taskbar preview reset.", "Preview-only");
    }
}

internal sealed class DesignTimeDesktopWidgetService : IDesktopWidgetService
{
    private DesktopWidgetSettings settings = new();

    public bool IsWidgetsVisible { get; private set; }

    public int ActiveWidgetCount => IsWidgetsVisible
        ? settings.Widgets.Count(widget => widget.IsEnabled && NordControl.Core.Modules.DesktopWidgetCatalog.IsImplementedWidgetType(widget.WidgetType))
        : 0;

    public IReadOnlyList<DesktopWidgetDefinition> GetDefinitions()
    {
        return NordControl.Core.Modules.DesktopWidgetCatalog.Definitions;
    }

    public DesktopWidgetSettings LoadSettings(AppSettings settings)
    {
        settings.Normalize();
        return settings.Customization.DesktopWidgets;
    }

    public WidgetOperationResult SaveSettings(DesktopWidgetSettings settings)
    {
        settings.Normalize();
        this.settings = settings;
        return WidgetOperationResult.Succeeded("Design-time widget settings saved.");
    }

    public WidgetOperationResult ShowWidgets()
    {
        IsWidgetsVisible = true;
        return WidgetOperationResult.Succeeded("Design-time widget windows shown.");
    }

    public WidgetOperationResult HideWidgets()
    {
        IsWidgetsVisible = false;
        return WidgetOperationResult.Succeeded("Design-time widget windows hidden.");
    }

    public WidgetOperationResult ToggleWidgets()
    {
        return settings.EnableWidgets ? HideWidgets() : ShowWidgets();
    }

    public WidgetOperationResult ResetWidgetLayout()
    {
        settings.Widgets = NordControl.Core.Modules.DesktopWidgetCatalog.CreateDefaultWidgetInstances();
        return WidgetOperationResult.Succeeded("Design-time widget layout reset.");
    }

    public WidgetOperationResult SaveWidgetBounds(string widgetId, double x, double y, double width, double height)
    {
        var widget = settings.Widgets.FirstOrDefault(item => item.Id == widgetId);
        if (widget is null)
        {
            return WidgetOperationResult.Failed("Design-time widget was not found.");
        }

        widget.X = x;
        widget.Y = y;
        widget.Width = width;
        widget.Height = height;
        widget.Normalize();
        return WidgetOperationResult.Succeeded("Design-time widget bounds saved.");
    }

    public WidgetOperationResult HideWidget(string widgetId, bool disable)
    {
        if (disable)
        {
            var widget = settings.Widgets.FirstOrDefault(item => item.Id == widgetId);
            if (widget is not null)
            {
                widget.IsEnabled = false;
            }
        }

        return WidgetOperationResult.Succeeded("Design-time widget hidden.");
    }

    public string GetWidgetLayoutSummary()
    {
        return string.Join(", ", settings.Widgets.Select(widget => $"{widget.WidgetType}: {Math.Round(widget.X)},{Math.Round(widget.Y)}"));
    }
}
