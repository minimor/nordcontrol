using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface ITaskbarService
{
    TaskbarState GetCurrentState();

    IReadOnlyList<TaskbarPreset> GetPresets();

    TaskbarOperationResult PreviewPreset(string presetKey);

    TaskbarOperationResult ApplyPreset(string presetKey, bool allowMediumRisk);

    TaskbarOperationResult ResetPreview();
}
