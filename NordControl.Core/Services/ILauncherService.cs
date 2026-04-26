using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface ILauncherService
{
    bool CloseAfterAction { get; }

    IReadOnlyList<LauncherCommand> GetCommands();

    IReadOnlyList<LauncherSearchResult> Search(string query);

    LauncherOperationResult Execute(LauncherCommand command);

    LauncherOperationResult RegisterHotkey();

    LauncherOperationResult UnregisterHotkey();
}
