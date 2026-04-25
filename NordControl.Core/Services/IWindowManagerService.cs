using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface IWindowManagerService
{
    IReadOnlyList<WindowInfo> GetOpenWindows();

    WindowOperationResult SetTopMost(nint hwnd);

    WindowOperationResult RemoveTopMost(nint hwnd);

    bool IsTopMost(nint hwnd);
}
