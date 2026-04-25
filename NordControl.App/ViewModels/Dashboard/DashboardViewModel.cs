using System.ComponentModel;
using NordControl.App.ViewModels.WindowManager;

namespace NordControl.App.ViewModels.Dashboard;

public sealed class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly WindowManagerViewModel windowManagerViewModel;

    public DashboardViewModel()
        : this(new WindowManagerViewModel())
    {
    }

    public DashboardViewModel(WindowManagerViewModel windowManagerViewModel)
    {
        this.windowManagerViewModel = windowManagerViewModel;
        this.windowManagerViewModel.PropertyChanged += OnWindowManagerPropertyChanged;
    }

    public int OpenWindowCount => windowManagerViewModel.OpenWindowCount;

    public int TopMostWindowCount => windowManagerViewModel.TopMostWindowCount;

    public string CurrentProfile => windowManagerViewModel.CurrentProfile;

    public string WindowOperationMessage => windowManagerViewModel.WindowOperationMessage;

    public string LastRefreshText => windowManagerViewModel.LastRefreshText;

    public void Dispose()
    {
        windowManagerViewModel.PropertyChanged -= OnWindowManagerPropertyChanged;
    }

    private void OnWindowManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WindowManagerViewModel.OpenWindowCount))
        {
            OnPropertyChanged(nameof(OpenWindowCount));
        }

        if (e.PropertyName is nameof(WindowManagerViewModel.TopMostWindowCount))
        {
            OnPropertyChanged(nameof(TopMostWindowCount));
        }

        if (e.PropertyName is nameof(WindowManagerViewModel.WindowOperationMessage))
        {
            OnPropertyChanged(nameof(WindowOperationMessage));
        }

        if (e.PropertyName is nameof(WindowManagerViewModel.LastRefreshText))
        {
            OnPropertyChanged(nameof(LastRefreshText));
        }
    }
}
