using System;
using Avalonia.Controls;
using Avalonia.Input;
using NordControl.App.ViewModels.Shell;

namespace NordControl.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Dispose();
        }

        base.OnClosed(e);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        if (e.Key == Key.Space && e.KeyModifiers == KeyModifiers.Control)
        {
            viewModel.ToggleLauncher();
            e.Handled = true;
        }
    }
}
