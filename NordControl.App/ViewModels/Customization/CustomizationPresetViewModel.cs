using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;

namespace NordControl.App.ViewModels.Customization;

public sealed class CustomizationPresetViewModel
{
    public CustomizationPresetViewModel(
        CustomizationPreset preset,
        Action<CustomizationPresetViewModel> applyPreset)
    {
        Preset = preset;
        ApplyCommand = new RelayCommand(() => applyPreset(this));
    }

    public CustomizationPreset Preset { get; }

    public string Key => Preset.Key;

    public string Name => Preset.Name;

    public string Description => Preset.Description;

    public string AccentColorHex => Preset.AccentColorHex;

    public string BackgroundHint => Preset.BackgroundHint;

    public string StyleMood => Preset.StyleMood;

    public string RiskLevel => Preset.RiskLevel;

    public string Scope => Preset.AppliesToNordControlOnly ? "NordControl preview" : "Windows";

    public ICommand ApplyCommand { get; }
}
