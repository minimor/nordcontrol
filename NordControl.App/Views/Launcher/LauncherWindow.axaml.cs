using Avalonia.Controls;
using Avalonia.Input;
using NordControl.App.ViewModels.Launcher;

namespace NordControl.App.Views.Launcher;

public partial class LauncherWindow : Window
{
    public LauncherWindow()
    {
        InitializeComponent();
        Opened += (_, _) => SearchBox.Focus();
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not LauncherViewModel viewModel)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
            case Key.Enter:
                viewModel.ExecuteSelected();
                e.Handled = true;
                break;
            case Key.Down:
                viewModel.MoveSelection(1);
                e.Handled = true;
                break;
            case Key.Up:
                viewModel.MoveSelection(-1);
                e.Handled = true;
                break;
        }
    }
}
