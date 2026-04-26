using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NordControl.App.ViewModels;
using NordControl.Core.Services;

namespace NordControl.App.ViewModels.Launcher;

public partial class LauncherViewModel : ViewModelBase
{
    private readonly ILauncherService launcherService;
    private readonly Action close;

    public LauncherViewModel(ILauncherService launcherService, Action close)
    {
        this.launcherService = launcherService;
        this.close = close;
        RefreshResults();
    }

    public ObservableCollection<LauncherResultViewModel> Results { get; } = [];

    [ObservableProperty]
    private string query = string.Empty;

    [ObservableProperty]
    private LauncherResultViewModel? selectedResult;

    [ObservableProperty]
    private string statusMessage = "Type a command, app, or action.";

    public bool HasResults => Results.Count > 0;

    partial void OnQueryChanged(string value)
    {
        RefreshResults();
    }

    public void MoveSelection(int delta)
    {
        if (Results.Count == 0)
        {
            SelectedResult = null;
            return;
        }

        var currentIndex = SelectedResult is null ? -1 : Results.IndexOf(SelectedResult);
        var nextIndex = currentIndex + delta;
        if (nextIndex < 0)
        {
            nextIndex = Results.Count - 1;
        }
        else if (nextIndex >= Results.Count)
        {
            nextIndex = 0;
        }

        SelectedResult = Results[nextIndex];
    }

    public void ExecuteSelected()
    {
        if (SelectedResult is null)
        {
            return;
        }

        Execute(SelectedResult);
    }

    private void RefreshResults()
    {
        Results.Clear();
        foreach (var result in launcherService.Search(Query))
        {
            Results.Add(new LauncherResultViewModel(result, Execute));
        }

        SelectedResult = Results.FirstOrDefault();
        StatusMessage = Results.Count == 0 ? "No matching commands." : $"{Results.Count} result(s)";
        OnPropertyChanged(nameof(HasResults));
    }

    private void Execute(LauncherResultViewModel result)
    {
        var operation = launcherService.Execute(result.Command);
        StatusMessage = operation.Success
            ? operation.Message
            : $"{operation.Message}{(string.IsNullOrWhiteSpace(operation.Details) ? string.Empty : $" {operation.Details}")}";

        if (launcherService.CloseAfterAction)
        {
            close();
        }
    }
}
