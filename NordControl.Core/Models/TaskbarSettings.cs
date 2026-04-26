using NordControl.Core.Modules;

namespace NordControl.Core.Models;

public sealed class TaskbarSettings
{
    public string SelectedTaskbarPresetKey { get; set; } = TaskbarPresetCatalog.DefaultPresetKey;

    public bool EnableTaskbarLab { get; set; }

    public bool AllowMediumRiskTaskbarChanges { get; set; }

    public bool UsePreviewOnlyMode { get; set; } = true;

    public bool ShowTaskbarWarnings { get; set; } = true;

    public DateTime? LastAppliedAt { get; set; }

    public void Normalize()
    {
        if (!TaskbarPresetCatalog.IsKnownPresetKey(SelectedTaskbarPresetKey))
        {
            SelectedTaskbarPresetKey = TaskbarPresetCatalog.DefaultPresetKey;
        }

        if (!EnableTaskbarLab)
        {
            AllowMediumRiskTaskbarChanges = false;
        }
    }
}
