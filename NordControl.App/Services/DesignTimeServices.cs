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
