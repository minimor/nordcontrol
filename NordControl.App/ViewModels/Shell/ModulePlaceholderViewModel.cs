namespace NordControl.App.ViewModels.Shell;

public sealed class ModulePlaceholderViewModel(ShellModuleViewModel module) : ViewModelBase
{
    public string ModuleName { get; } = module.DisplayName;

    public string Description { get; } = "This module is prepared in the shell and will be implemented in a later stage.";
}
