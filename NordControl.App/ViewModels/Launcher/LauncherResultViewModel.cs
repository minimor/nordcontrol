using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;

namespace NordControl.App.ViewModels.Launcher;

public sealed class LauncherResultViewModel
{
    private readonly Action<LauncherResultViewModel> execute;

    public LauncherResultViewModel(LauncherSearchResult result, Action<LauncherResultViewModel> execute)
    {
        Result = result;
        this.execute = execute;
        ExecuteCommand = new RelayCommand(() => this.execute(this));
    }

    public LauncherSearchResult Result { get; }

    public LauncherCommand Command => Result.Command;

    public string Title => Command.Title;

    public string Subtitle => Command.Subtitle;

    public string Category => Command.Category;

    public string IconHint => Command.IconHint;

    public string RiskLevel => Command.RiskLevel;

    public IRelayCommand ExecuteCommand { get; }
}
