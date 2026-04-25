using NordControl.Core.Models;

namespace NordControl.App.ViewModels;

public sealed class ShellModuleViewModel(AppModule module)
{
    public string Key { get; } = module.Key;

    public string DisplayName { get; } = module.DisplayName;

    public string Description { get; } = module.Description;

    public string Status { get; } = module.Status;
}
