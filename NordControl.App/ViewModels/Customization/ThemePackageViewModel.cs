using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NordControl.Core.Models;

namespace NordControl.App.ViewModels.Customization;

public sealed partial class ThemePackageViewModel : ObservableObject
{
    public ThemePackageViewModel(
        ThemePackage theme,
        string source,
        Action<ThemePackageViewModel> previewTheme,
        Action<ThemePackageViewModel> applyTheme,
        Action<ThemePackageViewModel> exportTheme)
    {
        Theme = theme;
        Source = source;
        PreviewCommand = new RelayCommand(() => previewTheme(this));
        ApplyCommand = new RelayCommand(() => applyTheme(this));
        ExportCommand = new RelayCommand(() => exportTheme(this));
    }

    public ThemePackage Theme { get; }

    public string Source { get; }

    public string Key => Theme.Key;

    public string Name => Theme.Name;

    public string Description => Theme.Description;

    public string Author => Theme.Author;

    public string Version => Theme.Version;

    public string AccentColorHex => Theme.AccentColorHex;

    public string SecondaryAccentColorHex => Theme.SecondaryAccentColorHex;

    public string BackgroundColorHex => Theme.BackgroundColorHex;

    public string SurfaceColorHex => Theme.SurfaceColorHex;

    public string TextColorHex => Theme.TextColorHex;

    public string MutedTextColorHex => Theme.MutedTextColorHex;

    public string BorderColorHex => Theme.BorderColorHex;

    public bool EnableGlass => Theme.EnableGlass;

    public string GlassBadge => Theme.EnableGlass ? $"Glass {Theme.GlassOpacity:P0}" : "Solid";

    public string CornerRadiusText => $"{Theme.CornerRadius:0}px corners";

    public string Mood => Theme.Mood;

    public string TagsText => string.Join(" / ", Theme.Tags);

    [ObservableProperty]
    private bool isSelected;

    public ICommand PreviewCommand { get; }

    public ICommand ApplyCommand { get; }

    public ICommand ExportCommand { get; }
}
