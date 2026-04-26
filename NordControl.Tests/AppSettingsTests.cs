using NordControl.Core.Models;
using NordControl.Windows.Services;

namespace NordControl.Tests;

public sealed class AppSettingsTests
{
    [Fact]
    public void DefaultSettingsUseSafeInitialValues()
    {
        var settings = AppSettings.CreateDefault();

        Assert.Equal("Dark", settings.Theme);
        Assert.Equal("dashboard", settings.LastSelectedModuleKey);
        Assert.True(settings.WindowManager.RefreshOnStartup);
        Assert.Equal(0, settings.WindowManager.AutoRefreshIntervalSeconds);
        Assert.False(settings.WindowManager.ConfirmBeforePinning);
        Assert.True(settings.WindowManager.ShowUnknownProcesses);
        Assert.Equal("overview", settings.Customization.LastSelectedSectionKey);
        Assert.Equal("fluent-dark", settings.Customization.SelectedPresetKey);
        Assert.Equal("fluent-dark", settings.Customization.SelectedThemePackageKey);
        Assert.Equal(string.Empty, settings.Customization.LastExportedThemePath);
        Assert.Equal(string.Empty, settings.Customization.LastImportedThemePath);
        Assert.True(settings.Customization.ApplyThemeToNordControlShell);
        Assert.Equal("#4CC2FF", settings.Customization.NordControlAccentColorHex);
        Assert.True(settings.Customization.EnableGlassStyleInApp);
        Assert.True(settings.Customization.AllowLowRiskWindowsPersonalization);
        Assert.Equal("fluent-transparent", settings.Customization.Taskbar.SelectedTaskbarPresetKey);
        Assert.False(settings.Customization.Taskbar.EnableTaskbarLab);
        Assert.False(settings.Customization.Taskbar.AllowMediumRiskTaskbarChanges);
        Assert.True(settings.Customization.Taskbar.UsePreviewOnlyMode);
        Assert.True(settings.Customization.Taskbar.ShowTaskbarWarnings);
        Assert.False(settings.Customization.DesktopWidgets.EnableWidgets);
        Assert.False(settings.Customization.DesktopWidgets.StartWidgetsWithApp);
        Assert.False(settings.Customization.DesktopWidgets.LockWidgetPositions);
        Assert.True(settings.Customization.DesktopWidgets.ShowWidgetBackground);
        Assert.Equal(0.92, settings.Customization.DesktopWidgets.GlobalOpacity);
        Assert.Contains(settings.Customization.DesktopWidgets.Widgets, widget => widget.WidgetType == "clock");
        Assert.Contains(settings.Customization.DesktopWidgets.Widgets, widget => widget.WidgetType == "system-monitor-lite");
        Assert.True(settings.Launcher.EnableLauncher);
        Assert.True(settings.Launcher.StartWithApp);
        Assert.Equal("Ctrl+Space", settings.Launcher.HotkeyGesture);
        Assert.True(settings.Launcher.IncludeNordControlCommands);
        Assert.True(settings.Launcher.IncludeApps);
        Assert.True(settings.Launcher.IncludeSystemActions);
        Assert.True(settings.Launcher.CloseAfterAction);
        Assert.Equal(10, settings.Launcher.MaxResults);
    }

    [Fact]
    public void NormalizeClampsInvalidAutoRefreshInterval()
    {
        var settings = AppSettings.CreateDefault();
        settings.WindowManager.AutoRefreshIntervalSeconds = -10;
        settings.Customization.LastSelectedSectionKey = "not-a-section";
        settings.Customization.NordControlAccentColorHex = "blue-ish";
        settings.Customization.SelectedThemePackageKey = "";
        settings.Customization.Taskbar.SelectedTaskbarPresetKey = "not-a-taskbar-preset";
        settings.Customization.Taskbar.AllowMediumRiskTaskbarChanges = true;
        settings.Customization.DesktopWidgets.GlobalOpacity = -1;
        settings.Customization.DesktopWidgets.Widgets =
        [
            new DesktopWidgetInstanceSettings
            {
                Id = "unknown",
                WidgetType = "unknown-widget",
                IsEnabled = true
            }
        ];
        settings.Launcher.HotkeyGesture = "not-real";
        settings.Launcher.MaxResults = 1000;

        settings.Normalize();

        Assert.Equal(0, settings.WindowManager.AutoRefreshIntervalSeconds);
        Assert.Equal("overview", settings.Customization.LastSelectedSectionKey);
        Assert.Equal("#4CC2FF", settings.Customization.NordControlAccentColorHex);
        Assert.Equal("fluent-dark", settings.Customization.SelectedThemePackageKey);
        Assert.Equal("fluent-transparent", settings.Customization.Taskbar.SelectedTaskbarPresetKey);
        Assert.False(settings.Customization.Taskbar.AllowMediumRiskTaskbarChanges);
        Assert.Equal(0.2, settings.Customization.DesktopWidgets.GlobalOpacity);
        Assert.Contains(settings.Customization.DesktopWidgets.Widgets, widget => widget.WidgetType == "clock");
        Assert.Contains(settings.Customization.DesktopWidgets.Widgets, widget => widget.WidgetType == "system-monitor-lite");
        Assert.Contains(settings.Customization.DesktopWidgets.Widgets, widget => widget.WidgetType == "unknown-widget" && !widget.IsEnabled);
        Assert.Equal("Ctrl+Space", settings.Launcher.HotkeyGesture);
        Assert.Equal(20, settings.Launcher.MaxResults);
    }

    [Fact]
    public void NormalizePreservesUserThemePackageKey()
    {
        var settings = AppSettings.CreateDefault();
        settings.Customization.SelectedThemePackageKey = "my-imported-theme";

        settings.Normalize();

        Assert.Equal("my-imported-theme", settings.Customization.SelectedThemePackageKey);
    }

    [Fact]
    public void JsonSettingsServiceCreatesAndLoadsSettingsFromCustomPath()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var settingsPath = Path.Combine(tempDirectory, "settings.json");
            var service = new JsonAppSettingsService(settingsPath);

            var loaded = service.Load();
            loaded.LastSelectedModuleKey = "settings";
            loaded.WindowManager.RefreshOnStartup = false;
            service.Save(loaded);

            var reloaded = service.Load();

            Assert.True(File.Exists(settingsPath));
            Assert.Equal("settings", reloaded.LastSelectedModuleKey);
            Assert.False(reloaded.WindowManager.RefreshOnStartup);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void JsonSettingsServicePreservesBrokenJsonAndResetsToDefaults()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var settingsPath = Path.Combine(tempDirectory, "settings.json");
            File.WriteAllText(settingsPath, "{not valid json");

            var service = new JsonAppSettingsService(settingsPath);
            var settings = service.Load();

            Assert.Equal("Dark", settings.Theme);
            Assert.True(File.Exists(settingsPath));
            Assert.Contains("invalid", service.LastStatusMessage, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(Directory.GetFiles(tempDirectory, "settings.broken-*.json"));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void JsonSettingsServiceResetWritesDefaults()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var settingsPath = Path.Combine(tempDirectory, "settings.json");
            var service = new JsonAppSettingsService(settingsPath);
            var settings = AppSettings.CreateDefault();
            settings.LastSelectedModuleKey = "settings";
            service.Save(settings);

            var reset = service.ResetToDefaults();
            var reloaded = service.Load();

            Assert.Equal("dashboard", reset.LastSelectedModuleKey);
            Assert.Equal("dashboard", reloaded.LastSelectedModuleKey);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"NordControl.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
