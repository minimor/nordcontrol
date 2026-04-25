using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NordControl.App.Services;
using NordControl.Core.Models;
using NordControl.Core.Modules;
using NordControl.Core.Services;

namespace NordControl.App.ViewModels.Customization;

public partial class CustomizationViewModel : ViewModelBase, IDisposable
{
    private readonly IAppStateService appStateService;
    private readonly IWindowsPersonalizationService windowsPersonalizationService;
    private bool isApplyingSettings;

    public CustomizationViewModel()
        : this(
            new AppStateService(new DesignTimeAppSettingsService()),
            new DesignTimeWindowsPersonalizationService())
    {
    }

    public CustomizationViewModel(
        IAppStateService appStateService,
        IWindowsPersonalizationService windowsPersonalizationService)
    {
        this.appStateService = appStateService;
        this.windowsPersonalizationService = windowsPersonalizationService;

        CustomizationPresets = CustomizationPresetCatalog.DefaultPresets
            .Select(preset => new CustomizationPresetViewModel(preset, ApplyCustomizationPreset))
            .ToList();
        CustomizationSections = CustomizationSectionCatalog.Sections
            .Select(section => new CustomizationSectionViewModel(section, SelectCustomizationSection))
            .ToList();

        this.appStateService.SettingsChanged += OnSettingsChanged;
        ApplySettings();
        LoadPersonalizationState();
    }

    public IReadOnlyList<CustomizationPresetViewModel> CustomizationPresets { get; }

    public IReadOnlyList<CustomizationSectionViewModel> CustomizationSections { get; }

    public ObservableCollection<CustomizationFeatureCardViewModel> CurrentCustomizationFeatureCards { get; } = [];

    [ObservableProperty]
    private string personalizationStatusMessage = "Customization Studio ready.";

    [ObservableProperty]
    private string appsTheme = "Unknown";

    [ObservableProperty]
    private string systemTheme = "Unknown";

    [ObservableProperty]
    private bool transparencyEffectsEnabled;

    [ObservableProperty]
    private string windowsAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool accentColorOnTitleBars;

    [ObservableProperty]
    private string wallpaperPath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PersonalizationLastLoadedText))]
    private DateTime? personalizationLastLoadedAt;

    [ObservableProperty]
    private string selectedCustomizationPresetKey = "fluent-dark";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentCustomizationSectionName))]
    [NotifyPropertyChangedFor(nameof(CurrentCustomizationSectionDescription))]
    [NotifyPropertyChangedFor(nameof(CurrentCustomizationSectionBadge))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationOverviewSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationThemesSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationPlanningSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationRiskLabSection))]
    private CustomizationSectionViewModel? selectedCustomizationSection;

    [ObservableProperty]
    private string nordControlAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool enableGlassStyleInApp = true;

    [ObservableProperty]
    private bool allowLowRiskWindowsPersonalization = true;

    public string PersonalizationLastLoadedText =>
        PersonalizationLastLoadedAt?.ToString("HH:mm:ss") ?? "Not loaded";

    public string CurrentCustomizationSectionName => SelectedCustomizationSection?.Name ?? "Overview";

    public string CurrentCustomizationSectionDescription =>
        SelectedCustomizationSection?.Description ?? "Snapshot, safety status, and the desktop environment roadmap.";

    public string CurrentCustomizationSectionBadge => SelectedCustomizationSection?.Badge ?? "Safe Layer";

    public bool IsCustomizationOverviewSection => SelectedCustomizationSection?.Key == "overview";

    public bool IsCustomizationThemesSection => SelectedCustomizationSection?.Key == "themes";

    public bool IsCustomizationRiskLabSection => SelectedCustomizationSection?.IsRiskLab == true;

    public bool IsCustomizationPlanningSection =>
        !IsCustomizationOverviewSection && !IsCustomizationThemesSection;

    partial void OnSelectedCustomizationSectionChanged(CustomizationSectionViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        appStateService.Settings.Customization.LastSelectedSectionKey = value.Key;
        if (!isApplyingSettings)
        {
            appStateService.Save();
        }

        RefreshCustomizationFeatureCards();
    }

    [RelayCommand]
    private void RefreshPersonalization()
    {
        LoadPersonalizationState();
    }

    [RelayCommand]
    private void ToggleAppsTheme()
    {
        if (!CanApplyLowRiskWindowsPersonalization())
        {
            return;
        }

        var nextTheme = AppsTheme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
        ApplyPersonalizationOperation(windowsPersonalizationService.SetAppsTheme(nextTheme));
    }

    [RelayCommand]
    private void ToggleSystemTheme()
    {
        if (!CanApplyLowRiskWindowsPersonalization())
        {
            return;
        }

        var nextTheme = SystemTheme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
        ApplyPersonalizationOperation(windowsPersonalizationService.SetSystemTheme(nextTheme));
    }

    [RelayCommand]
    private void ToggleTransparencyEffects()
    {
        if (!CanApplyLowRiskWindowsPersonalization())
        {
            return;
        }

        ApplyPersonalizationOperation(windowsPersonalizationService.SetTransparencyEffects(!TransparencyEffectsEnabled));
    }

    [RelayCommand]
    private void ToggleAccentColorOnTitleBars()
    {
        if (!CanApplyLowRiskWindowsPersonalization())
        {
            return;
        }

        ApplyPersonalizationOperation(windowsPersonalizationService.SetAccentColorOnTitleBars(!AccentColorOnTitleBars));
    }

    public void Dispose()
    {
        appStateService.SettingsChanged -= OnSettingsChanged;
    }

    private void ApplySettings()
    {
        var customizationSettings = appStateService.Settings.Customization;
        isApplyingSettings = true;

        SelectedCustomizationPresetKey = customizationSettings.SelectedPresetKey;
        SelectedCustomizationSection = CustomizationSections
            .FirstOrDefault(section => section.Key == customizationSettings.LastSelectedSectionKey)
            ?? CustomizationSections.FirstOrDefault(section => section.Key == CustomizationSectionCatalog.DefaultSectionKey)
            ?? CustomizationSections.FirstOrDefault();
        NordControlAccentColorHex = customizationSettings.NordControlAccentColorHex;
        EnableGlassStyleInApp = customizationSettings.EnableGlassStyleInApp;
        AllowLowRiskWindowsPersonalization = customizationSettings.AllowLowRiskWindowsPersonalization;

        isApplyingSettings = false;
        RefreshCustomizationFeatureCards();
    }

    private void LoadPersonalizationState()
    {
        try
        {
            var state = windowsPersonalizationService.GetCurrentState();
            AppsTheme = state.AppsTheme;
            SystemTheme = state.SystemTheme;
            TransparencyEffectsEnabled = state.TransparencyEffectsEnabled;
            WindowsAccentColorHex = state.AccentColorHex;
            AccentColorOnTitleBars = state.AccentColorOnTitleBars;
            WallpaperPath = string.IsNullOrWhiteSpace(state.WallpaperPath) ? "Not available" : state.WallpaperPath;
            PersonalizationLastLoadedAt = state.LastLoadedAt;
            PersonalizationStatusMessage = "Windows style snapshot refreshed.";
        }
        catch (Exception ex)
        {
            PersonalizationStatusMessage = $"Could not load Windows style: {ex.Message}";
        }
    }

    private void ApplyPersonalizationOperation(PersonalizationOperationResult result)
    {
        PersonalizationStatusMessage = result.Success
            ? result.Message
            : $"{result.Message}{(string.IsNullOrWhiteSpace(result.Requires) ? string.Empty : $" Requires: {result.Requires}.")}";

        LoadPersonalizationState();
        if (!result.Success)
        {
            PersonalizationStatusMessage = $"{result.Message}{(string.IsNullOrWhiteSpace(result.Requires) ? string.Empty : $" Requires: {result.Requires}.")}";
        }
    }

    private bool CanApplyLowRiskWindowsPersonalization()
    {
        if (AllowLowRiskWindowsPersonalization)
        {
            return true;
        }

        PersonalizationStatusMessage = "Low-risk Windows personalization is disabled in settings.";
        return false;
    }

    private void ApplyCustomizationPreset(CustomizationPresetViewModel preset)
    {
        SelectedCustomizationPresetKey = preset.Key;
        NordControlAccentColorHex = preset.AccentColorHex;
        EnableGlassStyleInApp = true;
        appStateService.Settings.Customization.SelectedPresetKey = preset.Key;
        appStateService.Settings.Customization.NordControlAccentColorHex = preset.AccentColorHex;
        appStateService.Settings.Customization.EnableGlassStyleInApp = true;
        appStateService.Save();
        PersonalizationStatusMessage = $"{preset.Name} applied to NordControl preview and saved.";
    }

    private void SelectCustomizationSection(CustomizationSectionViewModel section)
    {
        SelectedCustomizationSection = section;
    }

    private void RefreshCustomizationFeatureCards()
    {
        CurrentCustomizationFeatureCards.Clear();

        var sectionKey = SelectedCustomizationSection?.Key ?? CustomizationSectionCatalog.DefaultSectionKey;
        foreach (var card in CustomizationSectionCatalog.GetFeatureCards(sectionKey))
        {
            CurrentCustomizationFeatureCards.Add(new CustomizationFeatureCardViewModel(card));
        }
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        ApplySettings();
    }
}
