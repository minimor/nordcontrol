using Microsoft.Win32;
using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Core.Services;
using System.Runtime.Versioning;

namespace NordControl.Windows.Services;

public sealed class WindowsTaskbarService : ITaskbarService
{
    private const string ExplorerAdvancedKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string ExplorerStuckRectsKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3";
    private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public TaskbarState GetCurrentState()
    {
        if (!OperatingSystem.IsWindows())
        {
            return new TaskbarState(
                false,
                "Unavailable",
                "Unknown",
                "Unknown",
                "Unavailable",
                "Taskbar detection is only available on Windows.",
                DateTime.Now);
        }

        try
        {
            using var advancedKey = Registry.CurrentUser.OpenSubKey(ExplorerAdvancedKeyPath);
            using var stuckRectsKey = Registry.CurrentUser.OpenSubKey(ExplorerStuckRectsKeyPath);
            using var personalizeKey = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);

            return new TaskbarState(
                true,
                ReadTaskbarAlignment(advancedKey),
                ReadAutoHide(stuckRectsKey),
                ReadSmallTaskbarButtons(advancedKey),
                ReadTransparencyMode(personalizeKey),
                "Read-only HKCU snapshot. NordControl did not restart Explorer or change shell state.",
                DateTime.Now);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        {
            return new TaskbarState(
                true,
                "Unknown",
                "Unknown",
                "Unknown",
                "Unknown",
                $"Could not read taskbar state safely: {ex.Message}",
                DateTime.Now);
        }
    }

    public IReadOnlyList<TaskbarPreset> GetPresets()
    {
        return TaskbarPresetCatalog.Presets;
    }

    public TaskbarOperationResult PreviewPreset(string presetKey)
    {
        var preset = TaskbarPresetCatalog.GetPresetOrDefault(presetKey);
        return TaskbarOperationResult.Succeeded(
            $"{preset.Name} loaded in the app-only taskbar preview.",
            preset.RiskLevel,
            "No Windows shell changes were made.");
    }

    public TaskbarOperationResult ApplyPreset(string presetKey, bool allowMediumRisk)
    {
        if (!TaskbarPresetCatalog.IsKnownPresetKey(presetKey))
        {
            return TaskbarOperationResult.Failed(
                "That taskbar preset is not available. The preview was reset to the default preset.",
                "Preview-only",
                presetKey);
        }

        var preset = TaskbarPresetCatalog.GetPresetOrDefault(presetKey);
        if (preset.IsPreviewOnly || !preset.IsImplemented)
        {
            return TaskbarOperationResult.Failed(
                $"{preset.Name} is preview-only in Taskbar Lab V1. Real taskbar blur, replacement, hooks, and Explorer restarts remain future Risk Lab work.",
                preset.RiskLevel,
                "No Explorer patching, hooks, registry writes, or restarts were performed.");
        }

        if (preset.RiskLevel.Contains("Medium", StringComparison.OrdinalIgnoreCase) && !allowMediumRisk)
        {
            return TaskbarOperationResult.Failed(
                "Medium-risk taskbar changes are disabled in settings.",
                "Medium",
                "Enable the explicit medium-risk toggle before applying this kind of change.");
        }

        return TaskbarOperationResult.Failed(
            $"{preset.Name} has no Windows writer in V1.",
            preset.RiskLevel,
            "Taskbar Lab V1 is preview-first.");
    }

    public TaskbarOperationResult ResetPreview()
    {
        return TaskbarOperationResult.Succeeded(
            "Taskbar preview reset to the default preset.",
            "Preview-only");
    }

    [SupportedOSPlatform("windows")]
    private static string ReadTaskbarAlignment(RegistryKey? advancedKey)
    {
        return ReadInt(advancedKey, "TaskbarAl") switch
        {
            0 => "Left",
            1 => "Center",
            _ => "Unknown"
        };
    }

    [SupportedOSPlatform("windows")]
    private static string ReadSmallTaskbarButtons(RegistryKey? advancedKey)
    {
        return ReadInt(advancedKey, "TaskbarSmallIcons") switch
        {
            0 => "Disabled",
            1 => "Enabled",
            _ => "Unknown"
        };
    }

    [SupportedOSPlatform("windows")]
    private static string ReadAutoHide(RegistryKey? stuckRectsKey)
    {
        if (stuckRectsKey?.GetValue("Settings") is not byte[] settings || settings.Length <= 8)
        {
            return "Unknown";
        }

        return (settings[8] & 0x08) == 0x08 ? "Enabled" : "Disabled";
    }

    [SupportedOSPlatform("windows")]
    private static string ReadTransparencyMode(RegistryKey? personalizeKey)
    {
        return ReadInt(personalizeKey, "EnableTransparency") switch
        {
            0 => "Opaque",
            1 => "Transparency effects enabled",
            _ => "Unknown"
        };
    }

    [SupportedOSPlatform("windows")]
    private static int? ReadInt(RegistryKey? key, string valueName)
    {
        return key?.GetValue(valueName) is int value ? value : null;
    }
}
