using NordControl.Core.Modules;
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
}
