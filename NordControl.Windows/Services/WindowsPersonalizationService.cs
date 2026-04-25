using Microsoft.Win32;
using NordControl.Core.Models;
using NordControl.Core.Services;
using System.Runtime.Versioning;

namespace NordControl.Windows.Services;

public sealed class WindowsPersonalizationService : IWindowsPersonalizationService
{
    private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string DwmKeyPath = @"Software\Microsoft\Windows\DWM";
    private const string DesktopKeyPath = @"Control Panel\Desktop";

    public WindowsPersonalizationState GetCurrentState()
    {
        if (!OperatingSystem.IsWindows())
        {
            return new WindowsPersonalizationState(
                "Unavailable",
                "Unavailable",
                false,
                "#4CC2FF",
                false,
                "Windows personalization is unavailable on this platform.",
                DateTime.Now);
        }

        using var personalizeKey = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);
        using var dwmKey = Registry.CurrentUser.OpenSubKey(DwmKeyPath);
        using var desktopKey = Registry.CurrentUser.OpenSubKey(DesktopKeyPath);

        return new WindowsPersonalizationState(
            ReadTheme(personalizeKey, "AppsUseLightTheme"),
            ReadTheme(personalizeKey, "SystemUsesLightTheme"),
            ReadInt(personalizeKey, "EnableTransparency", defaultValue: 1) != 0,
            ReadAccentColor(dwmKey),
            ReadInt(dwmKey, "ColorPrevalence", defaultValue: 0) != 0,
            desktopKey?.GetValue("Wallpaper") as string ?? string.Empty,
            DateTime.Now);
    }

    public PersonalizationOperationResult SetAppsTheme(string theme)
    {
        return SetThemeValue("AppsUseLightTheme", theme, "Apps theme updated.");
    }

    public PersonalizationOperationResult SetSystemTheme(string theme)
    {
        return SetThemeValue("SystemUsesLightTheme", theme, "System theme updated.");
    }

    public PersonalizationOperationResult SetTransparencyEffects(bool enabled)
    {
        return WriteCurrentUserDword(
            PersonalizeKeyPath,
            "EnableTransparency",
            enabled ? 1 : 0,
            enabled ? "Transparency effects enabled." : "Transparency effects disabled.");
    }

    public PersonalizationOperationResult SetAccentColor(string hexColor)
    {
        return PersonalizationOperationResult.Failed(
            "Applying the Windows system accent color is coming later. The selected preset currently updates the NordControl preview only.",
            "App-only",
            "Safe accent color conversion and Windows refresh behavior");
    }

    public PersonalizationOperationResult SetAccentColorOnTitleBars(bool enabled)
    {
        return WriteCurrentUserDword(
            DwmKeyPath,
            "ColorPrevalence",
            enabled ? 1 : 0,
            enabled ? "Accent color enabled on title bars." : "Accent color disabled on title bars.");
    }

    public PersonalizationOperationResult SetWallpaper(string filePath)
    {
        return PersonalizationOperationResult.Failed(
            "Wallpaper changes are planned for a future version with preview and rollback support.",
            "App-only",
            "Wallpaper preview and explicit confirmation");
    }

    private static PersonalizationOperationResult SetThemeValue(string valueName, string theme, string message)
    {
        if (!OperatingSystem.IsWindows())
        {
            return PersonalizationOperationResult.Failed(
                "Windows personalization is unavailable on this platform.",
                "Unavailable",
                "Windows");
        }

        var normalizedTheme = theme.Equals("Light", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
        var value = normalizedTheme == "Light" ? 1 : 0;

        return WriteCurrentUserDword(PersonalizeKeyPath, valueName, value, message);
    }

    private static PersonalizationOperationResult WriteCurrentUserDword(
        string keyPath,
        string valueName,
        int value,
        string successMessage)
    {
        if (!OperatingSystem.IsWindows())
        {
            return PersonalizationOperationResult.Failed(
                "Windows personalization is unavailable on this platform.",
                "Unavailable",
                "Windows");
        }

        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            if (key is null)
            {
                return PersonalizationOperationResult.Failed(
                    "Could not open the current-user personalization settings.",
                    "Low",
                    details: keyPath);
            }

            key.SetValue(valueName, value, RegistryValueKind.DWord);

            return PersonalizationOperationResult.Succeeded(successMessage);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        {
            return PersonalizationOperationResult.Failed(
                "Windows rejected the personalization change.",
                "Low",
                details: ex.Message);
        }
    }

    private static bool IsWindowsAvailable(out PersonalizationOperationResult result)
    {
        if (OperatingSystem.IsWindows())
        {
            result = PersonalizationOperationResult.Succeeded("Windows personalization is available.");
            return true;
        }

        result = PersonalizationOperationResult.Failed(
            "Windows personalization is unavailable on this platform.",
            "Unavailable",
            "Windows");
        return false;
    }

    [SupportedOSPlatform("windows")]
    private static string ReadTheme(RegistryKey? key, string valueName)
    {
        return ReadInt(key, valueName, defaultValue: 0) == 1 ? "Light" : "Dark";
    }

    [SupportedOSPlatform("windows")]
    private static int ReadInt(RegistryKey? key, string valueName, int defaultValue)
    {
        return key?.GetValue(valueName) is int value ? value : defaultValue;
    }

    [SupportedOSPlatform("windows")]
    private static string ReadAccentColor(RegistryKey? key)
    {
        if (key?.GetValue("AccentColor") is not int rawColor)
        {
            return "#4CC2FF";
        }

        var unsigned = unchecked((uint)rawColor);
        var red = unsigned & 0xFF;
        var green = (unsigned >> 8) & 0xFF;
        var blue = (unsigned >> 16) & 0xFF;

        return $"#{red:X2}{green:X2}{blue:X2}";
    }
}
