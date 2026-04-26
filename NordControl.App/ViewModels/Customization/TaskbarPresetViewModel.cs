using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;

namespace NordControl.App.ViewModels.Customization;

public sealed class TaskbarPresetViewModel
{
    private readonly Action<TaskbarPresetViewModel> preview;
    private readonly Action<TaskbarPresetViewModel> apply;

    public TaskbarPresetViewModel(
        TaskbarPreset preset,
        Action<TaskbarPresetViewModel> preview,
        Action<TaskbarPresetViewModel> apply)
    {
        Preset = preset;
        this.preview = preview;
        this.apply = apply;
        PreviewCommand = new RelayCommand(() => this.preview(this));
        ApplyCommand = new RelayCommand(() => this.apply(this));
    }

    public TaskbarPreset Preset { get; }

    public string Key => Preset.Key;

    public string Name => Preset.Name;

    public string Description => Preset.Description;

    public string VisualStyle => Preset.VisualStyle;

    public string RiskLevel => Preset.RiskLevel;

    public string AccentColorHex => Preset.AccentColorHex;

    public string ModeBadge => Preset.IsPreviewOnly || !Preset.IsImplemented ? "Preview-only" : "Implemented";

    public string RestartBadge => Preset.RequiresExplorerRestart ? "Explorer restart needed" : "No Explorer restart";

    public IRelayCommand PreviewCommand { get; }

    public IRelayCommand ApplyCommand { get; }
}
