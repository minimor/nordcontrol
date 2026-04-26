using Avalonia.Controls;
using Avalonia.Input;
using NordControl.App.ViewModels.Widgets;

namespace NordControl.App.Views.Widgets;

public partial class DesktopWidgetWindow : Window
{
    public DesktopWidgetWindow()
    {
        InitializeComponent();
        PositionChanged += (_, _) => SaveCurrentBounds();
        SizeChanged += (_, _) => SaveCurrentBounds();
    }

    private void OnDragSurfacePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not DesktopWidgetWindowViewModel viewModel || viewModel.LockWidgetPositions)
        {
            return;
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void SaveCurrentBounds()
    {
        if (DataContext is not DesktopWidgetWindowViewModel viewModel)
        {
            return;
        }

        viewModel.SaveBounds(Position.X, Position.Y, Bounds.Width, Bounds.Height);
    }
}
