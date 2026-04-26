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
    private readonly IThemePackageService themePackageService;
    private readonly IWindowsPersonalizationService windowsPersonalizationService;
    private bool isApplyingSettings;

    public CustomizationViewModel()
        : this(
            new AppStateService(new DesignTimeAppSettingsService()),
            new JsonThemePackageService(),
            new DesignTimeWindowsPersonalizationService())
    {
    }

    public CustomizationViewModel(
        IAppStateService appStateService,
        IThemePackageService themePackageService,
        IWindowsPersonalizationService windowsPersonalizationService)
    {
        this.appStateService = appStateService;
        this.themePackageService = themePackageService;
        this.windowsPersonalizationService = windowsPersonalizationService;

        CustomizationPresets = CustomizationPresetCatalog.DefaultPresets
            .Select(preset => new CustomizationPresetViewModel(preset, ApplyCustomizationPreset))
            .ToList();
        CustomizationSections = CustomizationSectionCatalog.Sections
            .Select(section => new CustomizationSectionViewModel(section, SelectCustomizationSection))
            .ToList();

        this.appStateService.SettingsChanged += OnSettingsChanged;
        ResetCustomThemeEditor();
        ReloadThemePackages();
        ApplySettings();
        LoadPersonalizationState();
    }

    public ObservableCollection<ThemePackageViewModel> BuiltInThemePackages { get; } = [];

    public ObservableCollection<ThemePackageViewModel> UserThemePackages { get; } = [];

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
    private string selectedThemePackageKey = "fluent-dark";

    [ObservableProperty]
    private string selectedThemePackageName = "Fluent Dark";

    [ObservableProperty]
    private string lastExportedThemePath = string.Empty;

    [ObservableProperty]
    private string lastImportedThemePath = string.Empty;

    [ObservableProperty]
    private bool applyThemeToNordControlShell = true;

    [ObservableProperty]
    private string themeOperationStatus = "Theme packages ready.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImportPath))]
    private string importThemeFilePath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUserThemePackages))]
    [NotifyPropertyChangedFor(nameof(ShowNoUserThemePackages))]
    private int userThemePackageCount;

    [ObservableProperty]
    private string previewThemeName = "Fluent Dark";

    [ObservableProperty]
    private string previewAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private string previewSecondaryAccentColorHex = "#8CF5D2";

    [ObservableProperty]
    private string previewBackgroundColorHex = "#101418";

    [ObservableProperty]
    private string previewSurfaceColorHex = "#161D24";

    [ObservableProperty]
    private string previewTextColorHex = "#F4F7FA";

    [ObservableProperty]
    private string previewMutedTextColorHex = "#8FA1B3";

    [ObservableProperty]
    private string previewBorderColorHex = "#273442";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewGlassText))]
    private bool previewGlassEnabled = true;

    [ObservableProperty]
    private string customThemeName = "My Nord Theme";

    [ObservableProperty]
    private string customThemeDescription = "A custom NordControl theme package.";

    [ObservableProperty]
    private string customAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private string customBackgroundColorHex = "#101418";

    [ObservableProperty]
    private string customSurfaceColorHex = "#161D24";

    [ObservableProperty]
    private string customTextColorHex = "#F4F7FA";

    [ObservableProperty]
    private bool customEnableGlass = true;

    [ObservableProperty]
    private string customMood = "Custom";

    [ObservableProperty]
    private string customThemeValidationMessage = "Editor ready.";

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

    public string ThemesDirectoryPath => themePackageService.UserThemesDirectory;

    public bool HasImportPath => !string.IsNullOrWhiteSpace(ImportThemeFilePath);

    public bool HasUserThemePackages => UserThemePackageCount > 0;

    public bool ShowNoUserThemePackages => !HasUserThemePackages;

    public string PreviewGlassText => $"Glass enabled: {PreviewGlassEnabled}";

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
        SelectedThemePackageKey = customizationSettings.SelectedThemePackageKey;
        SelectedCustomizationSection = CustomizationSections
            .FirstOrDefault(section => section.Key == customizationSettings.LastSelectedSectionKey)
            ?? CustomizationSections.FirstOrDefault(section => section.Key == CustomizationSectionCatalog.DefaultSectionKey)
            ?? CustomizationSections.FirstOrDefault();
        LastExportedThemePath = customizationSettings.LastExportedThemePath;
        LastImportedThemePath = customizationSettings.LastImportedThemePath;
        ImportThemeFilePath = string.IsNullOrWhiteSpace(customizationSettings.LastImportedThemePath)
            ? customizationSettings.LastExportedThemePath
            : customizationSettings.LastImportedThemePath;
        ApplyThemeToNordControlShell = customizationSettings.ApplyThemeToNordControlShell;
        NordControlAccentColorHex = customizationSettings.NordControlAccentColorHex;
        EnableGlassStyleInApp = customizationSettings.EnableGlassStyleInApp;
        AllowLowRiskWindowsPersonalization = customizationSettings.AllowLowRiskWindowsPersonalization;

        isApplyingSettings = false;
        RefreshSelectedThemeState();
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

    private void PreviewThemePackage(ThemePackageViewModel themePackage)
    {
        ApplyThemePackageToPreview(themePackage.Theme, updateSelection: false);
        PersonalizationStatusMessage = $"{themePackage.Name} preview loaded. Apply it to save this theme package.";
    }

    private void ApplyThemePackage(ThemePackageViewModel themePackage)
    {
        ApplyThemePackageToPreview(themePackage.Theme, updateSelection: true);

        var customizationSettings = appStateService.Settings.Customization;
        customizationSettings.SelectedThemePackageKey = themePackage.Key;
        customizationSettings.NordControlAccentColorHex = themePackage.AccentColorHex;
        customizationSettings.EnableGlassStyleInApp = themePackage.EnableGlass;
        customizationSettings.ApplyThemeToNordControlShell = ApplyThemeToNordControlShell;

        if (CustomizationPresets.Any(preset => preset.Key == themePackage.Key))
        {
            customizationSettings.SelectedPresetKey = themePackage.Key;
            SelectedCustomizationPresetKey = themePackage.Key;
        }

        ApplySelectedThemeToSettings(themePackage.Theme);
        appStateService.Save();
        PersonalizationStatusMessage = $"{themePackage.Name} theme package applied and saved.";
        ThemeOperationStatus = $"{themePackage.Name} applied to NordControl preview.";
    }

    private void ExportThemePackage(ThemePackageViewModel themePackage)
    {
        var filePath = GetDefaultThemeExportPath(themePackage.Key);
        var result = themePackageService.ExportPackage(themePackage.Theme, filePath);

        PersonalizationStatusMessage = result.Success
            ? $"{result.Message} {result.FilePath}"
            : result.Message;
        ThemeOperationStatus = PersonalizationStatusMessage;

        if (result.Success && result.FilePath is not null)
        {
            LastExportedThemePath = result.FilePath;
            appStateService.Settings.Customization.LastExportedThemePath = result.FilePath;
            appStateService.Save();
        }
    }

    [RelayCommand]
    private void ExportSelectedThemePackage()
    {
        var theme = FindThemePackage(SelectedThemePackageKey) ?? themePackageService.GetSelectedPackage(appStateService.Settings);
        ExportThemePackage(CreateThemePackageViewModel(theme, ThemePackageCatalog.IsKnownThemeKey(theme.Key) ? "Built-in" : "User"));
    }

    [RelayCommand]
    private void ImportThemePackage()
    {
        var result = themePackageService.ImportPackage(ImportThemeFilePath);
        ThemeOperationStatus = result.Success
            ? $"{result.Message} Stored in {result.FilePath}"
            : result.Message;
        PersonalizationStatusMessage = ThemeOperationStatus;

        if (result.Success && result.FilePath is not null)
        {
            LastImportedThemePath = result.FilePath;
            appStateService.Settings.Customization.LastImportedThemePath = result.FilePath;
            ReloadThemePackages();
            appStateService.Save();
        }
    }

    [RelayCommand]
    private void PreviewCustomTheme()
    {
        if (!TryCreateCustomTheme(out var theme))
        {
            return;
        }

        ApplyThemePackageToPreview(theme, updateSelection: false);
        CustomThemeValidationMessage = $"{theme.Name} preview loaded.";
        ThemeOperationStatus = CustomThemeValidationMessage;
    }

    [RelayCommand]
    private void SaveCustomTheme()
    {
        if (!TryCreateCustomTheme(out var theme))
        {
            return;
        }

        var result = themePackageService.SaveUserPackage(theme);
        ThemeOperationStatus = result.Success
            ? $"{result.Message} {result.FilePath}"
            : result.Message;
        CustomThemeValidationMessage = ThemeOperationStatus;

        if (result.Success && result.FilePath is not null)
        {
            LastImportedThemePath = result.FilePath;
            appStateService.Settings.Customization.LastImportedThemePath = result.FilePath;
            ApplySelectedThemeToSettings(theme);
            ReloadThemePackages();
            appStateService.Save();
            PersonalizationStatusMessage = $"{theme.Name} saved as a user theme and applied.";
        }
    }

    [RelayCommand]
    private void ResetCustomThemeEditor()
    {
        CustomThemeName = "My Nord Theme";
        CustomThemeDescription = "A custom NordControl theme package.";
        CustomAccentColorHex = "#4CC2FF";
        CustomBackgroundColorHex = "#101418";
        CustomSurfaceColorHex = "#161D24";
        CustomTextColorHex = "#F4F7FA";
        CustomEnableGlass = true;
        CustomMood = "Custom";
        CustomThemeValidationMessage = "Editor reset.";
    }

    private void ApplyThemePackageToPreview(ThemePackage themePackage, bool updateSelection)
    {
        var normalizedTheme = themePackageService.Normalize(themePackage);

        if (updateSelection)
        {
            SelectedThemePackageKey = normalizedTheme.Key;
            SelectedThemePackageName = normalizedTheme.Name;
        }
        else
        {
            SelectedThemePackageName = $"{normalizedTheme.Name} preview";
        }

        if (ApplyThemeToNordControlShell)
        {
            NordControlAccentColorHex = normalizedTheme.AccentColorHex;
            EnableGlassStyleInApp = normalizedTheme.EnableGlass;
        }

        PreviewThemeName = normalizedTheme.Name;
        PreviewAccentColorHex = normalizedTheme.AccentColorHex;
        PreviewSecondaryAccentColorHex = normalizedTheme.SecondaryAccentColorHex;
        PreviewBackgroundColorHex = normalizedTheme.BackgroundColorHex;
        PreviewSurfaceColorHex = normalizedTheme.SurfaceColorHex;
        PreviewTextColorHex = normalizedTheme.TextColorHex;
        PreviewMutedTextColorHex = normalizedTheme.MutedTextColorHex;
        PreviewBorderColorHex = normalizedTheme.BorderColorHex;
        PreviewGlassEnabled = normalizedTheme.EnableGlass;

        if (updateSelection)
        {
            RefreshSelectedThemeState();
        }
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

    private void RefreshSelectedThemeState()
    {
        var selectedTheme = themePackageService.GetSelectedPackage(appStateService.Settings);
        SelectedThemePackageName = selectedTheme.Name;
        ApplyThemePackageToPreview(selectedTheme, updateSelection: false);
        SelectedThemePackageName = selectedTheme.Name;

        foreach (var themePackage in BuiltInThemePackages.Concat(UserThemePackages))
        {
            themePackage.IsSelected = string.Equals(
                themePackage.Key,
                SelectedThemePackageKey,
                StringComparison.Ordinal);
        }
    }

    private void ReloadThemePackages()
    {
        BuiltInThemePackages.Clear();
        foreach (var theme in themePackageService.GetBuiltInPackages())
        {
            BuiltInThemePackages.Add(CreateThemePackageViewModel(theme, "Built-in"));
        }

        UserThemePackages.Clear();
        foreach (var theme in themePackageService.GetUserPackages())
        {
            UserThemePackages.Add(CreateThemePackageViewModel(theme, "User"));
        }

        UserThemePackageCount = UserThemePackages.Count;
    }

    private ThemePackageViewModel CreateThemePackageViewModel(ThemePackage theme, string source)
    {
        return new ThemePackageViewModel(theme, source, PreviewThemePackage, ApplyThemePackage, ExportThemePackage);
    }

    private ThemePackage? FindThemePackage(string key)
    {
        return themePackageService.GetAllPackages()
            .FirstOrDefault(theme => string.Equals(theme.Key, key, StringComparison.Ordinal));
    }

    private void ApplySelectedThemeToSettings(ThemePackage theme)
    {
        var normalizedTheme = themePackageService.Normalize(theme);
        var customizationSettings = appStateService.Settings.Customization;
        customizationSettings.SelectedThemePackageKey = normalizedTheme.Key;
        customizationSettings.NordControlAccentColorHex = normalizedTheme.AccentColorHex;
        customizationSettings.EnableGlassStyleInApp = normalizedTheme.EnableGlass;
        customizationSettings.ApplyThemeToNordControlShell = ApplyThemeToNordControlShell;

        if (CustomizationPresets.Any(preset => preset.Key == normalizedTheme.Key))
        {
            customizationSettings.SelectedPresetKey = normalizedTheme.Key;
            SelectedCustomizationPresetKey = normalizedTheme.Key;
        }
    }

    private bool TryCreateCustomTheme(out ThemePackage theme)
    {
        theme = new ThemePackage
        {
            Key = CustomThemeName,
            Name = CustomThemeName,
            Description = CustomThemeDescription,
            Author = "NordControl User",
            Version = "1.0.0",
            AccentColorHex = CustomAccentColorHex,
            SecondaryAccentColorHex = CustomAccentColorHex,
            BackgroundColorHex = CustomBackgroundColorHex,
            SurfaceColorHex = CustomSurfaceColorHex,
            TextColorHex = CustomTextColorHex,
            MutedTextColorHex = "#8FA1B3",
            BorderColorHex = CustomAccentColorHex,
            EnableGlass = CustomEnableGlass,
            GlassOpacity = CustomEnableGlass ? 0.58 : 0,
            CornerRadius = 10,
            Mood = CustomMood,
            Tags = ["user", "custom"]
        };

        var invalidFields = new List<string>();
        if (!HexColorValidator.IsValidHexColor(CustomAccentColorHex))
        {
            invalidFields.Add("accent");
        }

        if (!HexColorValidator.IsValidHexColor(CustomBackgroundColorHex))
        {
            invalidFields.Add("background");
        }

        if (!HexColorValidator.IsValidHexColor(CustomSurfaceColorHex))
        {
            invalidFields.Add("surface");
        }

        if (!HexColorValidator.IsValidHexColor(CustomTextColorHex))
        {
            invalidFields.Add("text");
        }

        if (invalidFields.Count > 0)
        {
            CustomThemeValidationMessage = $"Fix invalid hex colors: {string.Join(", ", invalidFields)}.";
            ThemeOperationStatus = CustomThemeValidationMessage;
            return false;
        }

        theme = themePackageService.Normalize(theme);
        return true;
    }

    private string GetDefaultThemeExportPath(string themeKey)
    {
        return Path.Combine(themePackageService.UserThemesDirectory, $"{themeKey}.json");
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        ApplySettings();
    }
}
