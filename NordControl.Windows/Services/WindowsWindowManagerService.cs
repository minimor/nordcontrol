using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using NordControl.Core.Models;
using NordControl.Core.Services;

namespace NordControl.Windows.Services;

public sealed class WindowsWindowManagerService : IWindowManagerService
{
    private const int GwlExStyle = -20;
    private const long WsExTopMost = 0x00000008L;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;

    private static readonly nint HwndTopMost = new(-1);
    private static readonly nint HwndNoTopMost = new(-2);

    public IReadOnlyList<WindowInfo> GetOpenWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return [];
        }

        var windows = new List<WindowInfo>();
        var seenAt = DateTime.Now;

        EnumWindows((hwnd, lParam) =>
        {
            try
            {
                if (!IsWindowVisible(hwnd))
                {
                    return true;
                }

                var titleLength = GetWindowTextLength(hwnd);
                if (titleLength <= 0)
                {
                    return true;
                }

                var titleBuilder = new StringBuilder(titleLength + 1);
                var copiedLength = GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);
                var title = copiedLength > 0 ? titleBuilder.ToString() : string.Empty;
                if (string.IsNullOrWhiteSpace(title))
                {
                    return true;
                }

                _ = GetWindowThreadProcessId(hwnd, out var processId);
                var processName = ResolveProcessName(processId);

                windows.Add(new WindowInfo(
                    hwnd,
                    title,
                    processName,
                    processId == 0 ? null : unchecked((int)processId),
                    IsTopMost(hwnd),
                    seenAt));
            }
            catch
            {
                // Windows can close while being enumerated; skip volatile entries.
            }

            return true;
        }, nint.Zero);

        return windows
            .OrderBy(window => window.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public WindowOperationResult SetTopMost(nint hwnd)
    {
        return SetTopMostState(hwnd, HwndTopMost, "Window pinned as topmost.");
    }

    public WindowOperationResult RemoveTopMost(nint hwnd)
    {
        return SetTopMostState(hwnd, HwndNoTopMost, "Window returned to normal stacking.");
    }

    public bool IsTopMost(nint hwnd)
    {
        if (!OperatingSystem.IsWindows() || hwnd == nint.Zero)
        {
            return false;
        }

        try
        {
            var style = GetWindowLongPtr(hwnd, GwlExStyle);
            return ((long)style & WsExTopMost) == WsExTopMost;
        }
        catch
        {
            return false;
        }
    }

    private static WindowOperationResult SetTopMostState(nint hwnd, nint insertAfter, string successMessage)
    {
        if (!OperatingSystem.IsWindows())
        {
            return WindowOperationResult.Failed("Window pinning is only available on Windows.", handle: hwnd);
        }

        if (hwnd == nint.Zero)
        {
            return WindowOperationResult.Failed("The selected window is no longer available.", handle: hwnd);
        }

        var succeeded = SetWindowPos(
            hwnd,
            insertAfter,
            0,
            0,
            0,
            0,
            SwpNoMove | SwpNoSize | SwpNoActivate);

        if (succeeded)
        {
            return WindowOperationResult.Succeeded(successMessage, hwnd);
        }

        var errorCode = Marshal.GetLastWin32Error();
        var message = new Win32Exception(errorCode).Message;

        return WindowOperationResult.Failed(
            $"Window operation failed: {message}",
            errorCode,
            hwnd);
    }

    private static string ResolveProcessName(uint processId)
    {
        if (processId == 0)
        {
            return "Unknown";
        }

        try
        {
            using var process = Process.GetProcessById(unchecked((int)processId));
            return string.IsNullOrWhiteSpace(process.ProcessName)
                ? "Unknown"
                : process.ProcessName;
        }
        catch
        {
            return "Unknown";
        }
    }

    private static nint GetWindowLongPtr(nint hwnd, int index)
    {
        return nint.Size == 8
            ? GetWindowLongPtr64(hwnd, index)
            : GetWindowLongPtr32(hwnd, index);
    }

    private delegate bool EnumWindowsProc(nint hwnd, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern nint GetWindowLongPtr32(nint hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        nint hWnd,
        nint hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);
}
