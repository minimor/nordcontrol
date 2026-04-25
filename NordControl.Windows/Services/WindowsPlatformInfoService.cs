using System.Runtime.InteropServices;
using NordControl.Core.Models;
using NordControl.Core.Services;

namespace NordControl.Windows.Services;

public sealed class WindowsPlatformInfoService : IPlatformInfoService
{
    public PlatformInfo GetPlatformInfo()
    {
        return new PlatformInfo(
            RuntimeInformation.OSDescription,
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSArchitecture.ToString(),
            OperatingSystem.IsWindows());
    }
}
