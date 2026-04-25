using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;

namespace NordControl.App.ViewModels.WindowManager;

public sealed class WindowRowViewModel
{
    public WindowRowViewModel(
        WindowInfo window,
        Action<WindowRowViewModel> pinWindow,
        Action<WindowRowViewModel> unpinWindow)
    {
        Window = window;
        PinCommand = new RelayCommand(() => pinWindow(this));
        UnpinCommand = new RelayCommand(() => unpinWindow(this));
    }

    public WindowInfo Window { get; }

    public nint Handle => Window.Handle;

    public string HandleHex => $"0x{Handle.ToInt64():X}";

    public string Title => Window.Title;

    public string ProcessName => Window.ProcessName;

    public string ProcessId => Window.ProcessId?.ToString() ?? "-";

    public bool IsTopMost => Window.IsTopMost;

    public bool CanPin => !IsTopMost;

    public bool CanUnpin => IsTopMost;

    public string Status => IsTopMost ? "Topmost" : "Normal";

    public ICommand PinCommand { get; }

    public ICommand UnpinCommand { get; }
}
