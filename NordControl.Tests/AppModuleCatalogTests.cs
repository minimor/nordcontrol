using NordControl.Core.Modules;
using NordControl.Core.Models;
using NordControl.Core.Windows;
using NordControl.Windows.Services;

namespace NordControl.Tests;

public sealed class AppModuleCatalogTests
{
    [Fact]
    public void DefaultModulesContainTheInitialShellSections()
    {
        string[] expected =
        [
            "Dashboard",
            "Window Manager",
            "Performance Profiles",
            "Customization",
            "Settings"
        ];

        var actual = AppModuleCatalog.DefaultModules
            .Select(module => module.DisplayName)
            .ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DefaultModuleKeysAreStableAndUnique()
    {
        var keys = AppModuleCatalog.DefaultModules
            .Select(module => module.Key)
            .ToArray();

        Assert.All(keys, key => Assert.False(string.IsNullOrWhiteSpace(key)));
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void WindowsPlatformInfoServiceReportsRuntimeContext()
    {
        var service = new WindowsPlatformInfoService();

        var info = service.GetPlatformInfo();

        Assert.False(string.IsNullOrWhiteSpace(info.OperatingSystem));
        Assert.False(string.IsNullOrWhiteSpace(info.Runtime));
        Assert.False(string.IsNullOrWhiteSpace(info.Architecture));
    }

    [Fact]
    public void WindowInfoFilterSearchesTitleAndProcessName()
    {
        var windows = new[]
        {
            new WindowInfo(1, "Untitled - Notepad", "notepad", 100, false, DateTime.Now),
            new WindowInfo(2, "Project Board", "browser", 200, true, DateTime.Now)
        };

        var byTitle = WindowInfoFilter.Apply(windows, "untitled");
        var byProcess = WindowInfoFilter.Apply(windows, "browser");

        Assert.Single(byTitle);
        Assert.Equal("notepad", byTitle[0].ProcessName);
        Assert.Single(byProcess);
        Assert.Equal("Project Board", byProcess[0].Title);
    }

    [Fact]
    public void WindowInfoFilterReturnsAllWindowsForEmptySearch()
    {
        var windows = new[]
        {
            new WindowInfo(1, "One", "alpha", 100, false, DateTime.Now),
            new WindowInfo(2, "Two", "beta", 200, false, DateTime.Now)
        };

        var result = WindowInfoFilter.Apply(windows, " ");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void WindowOperationResultFactoriesSetStateAndHandle()
    {
        var success = WindowOperationResult.Succeeded("Pinned.", 42);
        var failure = WindowOperationResult.Failed("Failed.", 5, 42);

        Assert.True(success.Success);
        Assert.Equal((nint)42, success.Handle);
        Assert.False(failure.Success);
        Assert.Equal(5, failure.NativeErrorCode);
        Assert.Equal((nint)42, failure.Handle);
    }
}
