using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;

namespace NordControl.App.ViewModels.Customization;

public sealed class CustomizationSectionViewModel
{
    public CustomizationSectionViewModel(
        CustomizationSection section,
        Action<CustomizationSectionViewModel> selectSection)
    {
        Section = section;
        SelectCommand = new RelayCommand(() => selectSection(this));
    }

    public CustomizationSection Section { get; }

    public string Key => Section.Key;

    public string Name => Section.Name;

    public string Description => Section.Description;

    public string Badge => Section.Badge;

    public string AccentColorHex => Section.AccentColorHex;

    public bool IsRiskLab => Section.IsRiskLab;

    public ICommand SelectCommand { get; }
}
