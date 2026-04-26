using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;

namespace NordControl.App.ViewModels.Customization;

public sealed partial class DesktopWidgetDefinitionViewModel : ObservableObject
{
    private readonly Func<DesktopWidgetDefinitionViewModel, bool, bool> setEnabled;
    private readonly Action<DesktopWidgetDefinitionViewModel> show;

    public DesktopWidgetDefinitionViewModel(
        DesktopWidgetDefinition definition,
        bool isEnabled,
        Func<DesktopWidgetDefinitionViewModel, bool, bool> setEnabled,
        Action<DesktopWidgetDefinitionViewModel> show)
    {
        Definition = definition;
        this.setEnabled = setEnabled;
        this.show = show;
        this.isEnabled = isEnabled;
        ShowCommand = new RelayCommand(() => this.show(this), () => IsImplemented);
    }

    public DesktopWidgetDefinition Definition { get; }

    public string Key => Definition.Key;

    public string Name => Definition.Name;

    public string Description => Definition.Description;

    public string Category => Definition.Category;

    public string RiskLevel => Definition.RiskLevel;

    public string AccentColorHex => Definition.AccentColorHex;

    public bool IsImplemented => Definition.IsImplemented;

    public string ImplementationBadge => IsImplemented ? "Implemented" : "Planned";

    public IRelayCommand ShowCommand { get; }

    [ObservableProperty]
    private bool isEnabled;

    partial void OnIsEnabledChanged(bool value)
    {
        if (!setEnabled(this, value))
        {
            SetProperty(ref isEnabled, false, nameof(IsEnabled));
        }
    }
}
