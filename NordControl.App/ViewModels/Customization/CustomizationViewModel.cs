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
    private readonly ITaskbarService taskbarService;
    private readonly IDesktopWidgetService desktopWidgetService;
    private bool isApplyingSettings;

    public CustomizationViewModel()
        : this(
            new AppStateService(new DesignTimeAppSettingsService()),
            new JsonThemePackageService(),
            new DesignTimeWindowsPersonalizationService(),
            new DesignTimeTaskbarService(),
            new DesignTimeDesktopWidgetService())
    {
    }

    public CustomizationViewModel(
        IAppStateService appStateService,
        IThemePackageService themePackageService,
        IWindowsPersonalizationService windowsPersonalizationService,
        ITaskbarService taskbarService,
        IDesktopWidgetService desktopWidgetService)
    {
        this.appStateService = appStateService;
        this.themePackageService = themePackageService;
        this.windowsPersonalizationService = windowsPersonalizationService;
        this.taskbarService = taskbarService;
        this.desktopWidgetService = desktopWidgetService;

        CustomizationPresets = CustomizationPresetCatalog.DefaultPresets
            .Select(preset => new CustomizationPresetViewModel(preset, ApplyCustomizationPreset))
            .ToList();
        CustomizationSections = CustomizationSectionCatalog.Sections
            .Select(section => new CustomizationSectionViewModel(section, SelectCustomizationSection))
            .ToList();
        TaskbarPresets = this.taskbarService.GetPresets()
            .Select(preset => new TaskbarPresetViewModel(preset, PreviewTaskbarPreset, ApplyTaskbarPreset))
            .ToList();
        DesktopWidgetDefinitions = this.desktopWidgetService.GetDefinitions()
            .Select(definition => new DesktopWidgetDefinitionViewModel(
                definition,
                isEnabled: false,
                SetDesktopWidgetEnabled,
                ShowDesktopWidget))
            .ToList();

        this.appStateService.SettingsChanged += OnSettingsChanged;
        ResetCustomThemeEditor();
        ReloadThemePackages();
        ApplySettings();
        LoadPersonalizationState();
        LoadTaskbarState();
    }

    public ObservableCollection<ThemePackageViewModel> BuiltInThemePackages { get; } = [];

    public ObservableCollection<ThemePackageViewModel> UserThemePackages { get; } = [];

    public IReadOnlyList<CustomizationPresetViewModel> CustomizationPresets { get; }

    public IReadOnlyList<CustomizationSectionViewModel> CustomizationSections { get; }

    public IReadOnlyList<TaskbarPresetViewModel> TaskbarPresets { get; }

    public IReadOnlyList<DesktopWidgetDefinitionViewModel> DesktopWidgetDefinitions { get; }

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
    [NotifyPropertyChangedFor(nameof(IsCustomizationTaskbarSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationDesktopWidgetsSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationPlanningSection))]
    [NotifyPropertyChangedFor(nameof(IsCustomizationRiskLabSection))]
    private CustomizationSectionViewModel? selectedCustomizationSection;

    [ObservableProperty]
    private string nordControlAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private bool enableGlassStyleInApp = true;

    [ObservableProperty]
    private bool allowLowRiskWindowsPersonalization = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskbarLabRiskBadge))]
    private bool enableTaskbarLab;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskbarPreviewOnlyStatus))]
    private bool useTaskbarPreviewOnlyMode = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskbarLabRiskBadge))]
    private bool allowMediumRiskTaskbarChanges;

    [ObservableProperty]
    private bool showTaskbarWarnings = true;

    [ObservableProperty]
    private string selectedTaskbarPresetKey = TaskbarPresetCatalog.DefaultPresetKey;

    [ObservableProperty]
    private string selectedTaskbarPresetName = "Fluent Transparent";

    [ObservableProperty]
    private string taskbarStatusMessage = "Taskbar Lab ready. V1 is preview-only.";

    [ObservableProperty]
    private string taskbarIsWindowsText = "Unknown";

    [ObservableProperty]
    private string taskbarAlignment = "Unknown";

    [ObservableProperty]
    private string taskbarAutoHideStatus = "Unknown";

    [ObservableProperty]
    private string taskbarSmallButtonsStatus = "Unknown";

    [ObservableProperty]
    private string taskbarTransparencyMode = "Unknown";

    [ObservableProperty]
    private string taskbarNotes = "Taskbar snapshot not loaded yet.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskbarLastLoadedText))]
    private DateTime? taskbarLastLoadedAt;

    [ObservableProperty]
    private string taskbarPreviewAccentColorHex = "#4CC2FF";

    [ObservableProperty]
    private string taskbarPreviewDesktopColorHex = "#101418";

    [ObservableProperty]
    private string taskbarPreviewSurfaceColorHex = "#18202A";

    [ObservableProperty]
    private string taskbarPreviewRailColorHex = "#223040";

    [ObservableProperty]
    private string taskbarPreviewIconColorHex = "#D8E1EA";

    [ObservableProperty]
    private string taskbarPreviewDescription = "A clean translucent taskbar concept with soft Windows-style surfaces.";

    [ObservableProperty]
    private string taskbarPreviewVisualStyle = "Soft acrylic preview";

    [ObservableProperty]
    private string taskbarPreviewRiskLevel = "Preview-only";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DesktopWidgetsStatusBadge))]
    private bool enableDesktopWidgets;

    [ObservableProperty]
    private bool startWidgetsWithApp;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DesktopWidgetLockStatusText))]
    private bool lockWidgetPositions;

    [ObservableProperty]
    private bool showWidgetBackground = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DesktopWidgetPreviewOpacity))]
    private double desktopWidgetGlobalOpacity = 0.92;

    [ObservableProperty]
    private string selectedWidgetThemeKey = DesktopWidgetCatalog.DefaultWidgetThemeKey;

    [ObservableProperty]
    private string desktopWidgetStatusMessage = "Desktop Widgets ready. Overlay windows are app-owned.";

    [ObservableProperty]
    private string desktopWidgetPreviewBackgroundHex = "#18202A";

    [ObservableProperty]
    private string desktopWidgetPreviewAccentHex = "#A78BFA";

    [ObservableProperty]
    private string desktopWidgetVisibilityText = "Hidden";

    [ObservableProperty]
    private int activeDesktopWidgetCount;

    [ObservableProperty]
    private string desktopWidgetLayoutSummary = "Clock and System Monitor Lite use default positions.";

    public string PersonalizationLastLoadedText =>
        PersonalizationLastLoadedAt?.ToString("HH:mm:ss") ?? "Not loaded";

    public string CurrentCustomizationSectionName => SelectedCustomizationSection?.Name ?? "Overview";

    public string CurrentCustomizationSectionDescription =>
        SelectedCustomizationSection?.Description ?? "Snapshot, safety status, and the desktop environment roadmap.";

    public string CurrentCustomizationSectionBadge => SelectedCustomizationSection?.Badge ?? "Safe Layer";

    public bool IsCustomizationOverviewSection => SelectedCustomizationSection?.Key == "overview";

    public bool IsCustomizationThemesSection => SelectedCustomizationSection?.Key == "themes";

    public bool IsCustomizationTaskbarSection => SelectedCustomizationSection?.Key == "taskbar";

    public bool IsCustomizationDesktopWidgetsSection => SelectedCustomizationSection?.Key == "desktop-widgets";

    public bool IsCustomizationRiskLabSection => SelectedCustomizationSection?.IsRiskLab == true;

    public bool IsCustomizationPlanningSection =>
        !IsCustomizationOverviewSection &&
        !IsCustomizationThemesSection &&
        !IsCustomizationTaskbarSection &&
        !IsCustomizationDesktopWidgetsSection;

    public string ThemesDirectoryPath => themePackageService.UserThemesDirectory;

    public bool HasImportPath => !string.IsNullOrWhiteSpace(ImportThemeFilePath);

    public bool HasUserThemePackages => UserThemePackageCount > 0;

    public bool ShowNoUserThemePackages => !HasUserThemePackages;

    public string PreviewGlassText => $"Glass enabled: {PreviewGlassEnabled}";

    public string TaskbarLastLoadedText => TaskbarLastLoadedAt?.ToString("HH:mm:ss") ?? "Not loaded";

    public string TaskbarLabRiskBadge =>
        EnableTaskbarLab
            ? AllowMediumRiskTaskbarChanges ? "Medium gated" : "Preview-first"
            : "Disabled";

    public string TaskbarPreviewOnlyStatus => UseTaskbarPreviewOnlyMode ? "Preview-only mode on" : "Preview-only recommended";

    public string DesktopWidgetsStatusBadge => EnableDesktopWidgets ? "Safe Overlay On" : "Safe Overlay Off";

    public double DesktopWidgetPreviewOpacity => Math.Clamp(DesktopWidgetGlobalOpacity, 0.2, 1.0);

    public string DesktopWidgetLockStatusText => LockWidgetPositions ? "Positions locked" : "Positions unlocked";

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

    partial void OnEnableTaskbarLabChanged(bool value)
    {
        PersistTaskbarSettings(settings => settings.EnableTaskbarLab = value);
    }

    partial void OnUseTaskbarPreviewOnlyModeChanged(bool value)
    {
        PersistTaskbarSettings(settings => settings.UsePreviewOnlyMode = value);
    }

    partial void OnAllowMediumRiskTaskbarChangesChanged(bool value)
    {
        PersistTaskbarSettings(settings => settings.AllowMediumRiskTaskbarChanges = value);
    }

    partial void OnShowTaskbarWarningsChanged(bool value)
    {
        PersistTaskbarSettings(settings => settings.ShowTaskbarWarnings = value);
    }

    partial void OnEnableDesktopWidgetsChanged(bool value)
    {
        PersistDesktopWidgetSettings(settings => settings.EnableWidgets = value);
    }

    partial void OnStartWidgetsWithAppChanged(bool value)
    {
        PersistDesktopWidgetSettings(settings => settings.StartWidgetsWithApp = value);
    }

    partial void OnLockWidgetPositionsChanged(bool value)
    {
        PersistDesktopWidgetSettings(settings => settings.LockWidgetPositions = value);
    }

    partial void OnShowWidgetBackgroundChanged(bool value)
    {
        DesktopWidgetPreviewBackgroundHex = value ? "#18202A" : "#101418";
        PersistDesktopWidgetSettings(settings => settings.ShowWidgetBackground = value);
    }

    partial void OnDesktopWidgetGlobalOpacityChanged(double value)
    {
        PersistDesktopWidgetSettings(settings => settings.GlobalOpacity = value);
    }

    partial void OnSelectedWidgetThemeKeyChanged(string value)
    {
        PersistDesktopWidgetSettings(settings => settings.SelectedWidgetThemeKey = value);
    }

    [RelayCommand]
    private void RefreshPersonalization()
    {
        LoadPersonalizationState();
    }

    [RelayCommand]
    private void RefreshTaskbarState()
    {
        LoadTaskbarState();
    }

    [RelayCommand]
    private void ShowDesktopWidgets()
    {
        var result = desktopWidgetService.ShowWidgets();
        DesktopWidgetStatusMessage = BuildWidgetStatusMessage(result);
        RefreshDesktopWidgetRuntimeStatus();
    }

    [RelayCommand]
    private void HideDesktopWidgets()
    {
        var result = desktopWidgetService.HideWidgets();
        DesktopWidgetStatusMessage = BuildWidgetStatusMessage(result);
        RefreshDesktopWidgetRuntimeStatus();
    }

    [RelayCommand]
    private void ResetDesktopWidgetLayout()
    {
        var result = desktopWidgetService.ResetWidgetLayout();
        DesktopWidgetStatusMessage = BuildWidgetStatusMessage(result);
        ApplyDesktopWidgetSettings(appStateService.Settings.Customization.DesktopWidgets);
        RefreshDesktopWidgetRuntimeStatus();
    }

    [RelayCommand]
    private void ToggleDesktopWidgetPositionLock()
    {
        LockWidgetPositions = !LockWidgetPositions;
        DesktopWidgetStatusMessage = LockWidgetPositions
            ? "Widget positions locked."
            : "Widget positions unlocked. Drag widget headers to move them.";
    }

    [RelayCommand]
    private void SaveDesktopWidgetLayout()
    {
        var result = desktopWidgetService.SaveSettings(appStateService.Settings.Customization.DesktopWidgets);
        DesktopWidgetStatusMessage = BuildWidgetStatusMessage(result);
        RefreshDesktopWidgetRuntimeStatus();
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
        var taskbarSettings = customizationSettings.Taskbar;
        var widgetSettings = customizationSettings.DesktopWidgets;
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
        EnableTaskbarLab = taskbarSettings.EnableTaskbarLab;
        UseTaskbarPreviewOnlyMode = taskbarSettings.UsePreviewOnlyMode;
        AllowMediumRiskTaskbarChanges = taskbarSettings.AllowMediumRiskTaskbarChanges;
        ShowTaskbarWarnings = taskbarSettings.ShowTaskbarWarnings;
        SelectedTaskbarPresetKey = taskbarSettings.SelectedTaskbarPresetKey;
        ApplyDesktopWidgetSettings(widgetSettings);

        isApplyingSettings = false;
        RefreshSelectedThemeState();
        ApplyTaskbarPresetToPreview(SelectedTaskbarPresetKey);
        RefreshCustomizationFeatureCards();
        RefreshDesktopWidgetRuntimeStatus();
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

    private void LoadTaskbarState()
    {
        try
        {
            var state = taskbarService.GetCurrentState();
            TaskbarIsWindowsText = state.IsWindows ? "Windows detected" : "Not Windows";
            TaskbarAlignment = state.TaskbarAlignment;
            TaskbarAutoHideStatus = state.AutoHideEnabled;
            TaskbarSmallButtonsStatus = state.SmallTaskbarButtons;
            TaskbarTransparencyMode = state.TransparencyMode;
            TaskbarNotes = state.Notes;
            TaskbarLastLoadedAt = state.LastLoadedAt;
            TaskbarStatusMessage = "Taskbar snapshot refreshed.";
        }
        catch (Exception ex)
        {
            TaskbarStatusMessage = $"Could not load taskbar state: {ex.Message}";
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

    private void PreviewTaskbarPreset(TaskbarPresetViewModel preset)
    {
        SelectedTaskbarPresetKey = preset.Key;
        ApplyTaskbarPresetToPreview(preset.Key);
        var result = taskbarService.PreviewPreset(preset.Key);
        TaskbarStatusMessage = result.Message;
    }

    private void ApplyTaskbarPreset(TaskbarPresetViewModel preset)
    {
        SelectedTaskbarPresetKey = preset.Key;
        ApplyTaskbarPresetToPreview(preset.Key);

        var result = taskbarService.ApplyPreset(preset.Key, AllowMediumRiskTaskbarChanges);
        TaskbarStatusMessage = result.Success
            ? result.Message
            : $"{result.Message}{(string.IsNullOrWhiteSpace(result.Details) ? string.Empty : $" {result.Details}")}";

        var taskbarSettings = appStateService.Settings.Customization.Taskbar;
        taskbarSettings.SelectedTaskbarPresetKey = preset.Key;
        taskbarSettings.LastAppliedAt = DateTime.Now;
        appStateService.Save();
    }

    [RelayCommand]
    private void ResetTaskbarPreview()
    {
        var result = taskbarService.ResetPreview();
        SelectedTaskbarPresetKey = TaskbarPresetCatalog.DefaultPresetKey;
        ApplyTaskbarPresetToPreview(SelectedTaskbarPresetKey);
        appStateService.Settings.Customization.Taskbar.SelectedTaskbarPresetKey = SelectedTaskbarPresetKey;
        appStateService.Save();
        TaskbarStatusMessage = result.Message;
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

    private void ApplyTaskbarPresetToPreview(string presetKey)
    {
        var preset = TaskbarPresetCatalog.GetPresetOrDefault(presetKey);
        SelectedTaskbarPresetKey = preset.Key;
        SelectedTaskbarPresetName = preset.Name;
        TaskbarPreviewAccentColorHex = preset.AccentColorHex;
        TaskbarPreviewDescription = preset.Description;
        TaskbarPreviewVisualStyle = preset.VisualStyle;
        TaskbarPreviewRiskLevel = preset.RiskLevel;

        (TaskbarPreviewDesktopColorHex, TaskbarPreviewSurfaceColorHex, TaskbarPreviewRailColorHex, TaskbarPreviewIconColorHex) =
            preset.Key switch
            {
                "glass-floating" => ("#0B1719", "#20333A", "#2C4B51", "#E5FFF9"),
                "compact-focus" => ("#101316", "#171D23", "#202832", "#E8EDF2"),
                "productivity-bar" => ("#16140F", "#252217", "#3D341E", "#FFF2C2"),
                "cyber-neon" => ("#120B1A", "#211229", "#35183F", "#FFD9F6"),
                "minimal-dark" => ("#0E1116", "#151922", "#1D2430", "#DDE7FF"),
                _ => ("#101418", "#18202A", "#223040", "#D8E1EA")
            };
    }

    private void PersistTaskbarSettings(Action<TaskbarSettings> update)
    {
        if (isApplyingSettings)
        {
            return;
        }

        update(appStateService.Settings.Customization.Taskbar);
        appStateService.Save();
    }

    private void ApplyDesktopWidgetSettings(DesktopWidgetSettings settings)
    {
        EnableDesktopWidgets = settings.EnableWidgets;
        StartWidgetsWithApp = settings.StartWidgetsWithApp;
        LockWidgetPositions = settings.LockWidgetPositions;
        ShowWidgetBackground = settings.ShowWidgetBackground;
        DesktopWidgetGlobalOpacity = settings.GlobalOpacity;
        SelectedWidgetThemeKey = settings.SelectedWidgetThemeKey;
        DesktopWidgetPreviewBackgroundHex = settings.ShowWidgetBackground ? "#18202A" : "#101418";

        foreach (var definition in DesktopWidgetDefinitions)
        {
            definition.IsEnabled = settings.Widgets.Any(widget =>
                widget.IsEnabled &&
                string.Equals(widget.WidgetType, definition.Key, StringComparison.Ordinal));
        }

        RefreshDesktopWidgetRuntimeStatus();
    }

    private void PersistDesktopWidgetSettings(Action<DesktopWidgetSettings> update)
    {
        if (isApplyingSettings)
        {
            return;
        }

        var settings = appStateService.Settings.Customization.DesktopWidgets;
        update(settings);
        var result = desktopWidgetService.SaveSettings(settings);
        DesktopWidgetStatusMessage = BuildWidgetStatusMessage(result);
    }

    private bool SetDesktopWidgetEnabled(DesktopWidgetDefinitionViewModel definition, bool isEnabled)
    {
        if (isApplyingSettings)
        {
            return true;
        }

        if (!definition.IsImplemented)
        {
            DesktopWidgetStatusMessage = $"{definition.Name} is planned for a future widget stage.";
            return false;
        }

        var settings = appStateService.Settings.Customization.DesktopWidgets;
        var instance = settings.Widgets.FirstOrDefault(widget =>
            string.Equals(widget.WidgetType, definition.Key, StringComparison.Ordinal));

        if (instance is null)
        {
            instance = DesktopWidgetCatalog.CreateDefaultInstance(definition.Key);
            settings.Widgets.Add(instance);
        }

        instance.IsEnabled = isEnabled;
        var result = desktopWidgetService.SaveSettings(settings);
        DesktopWidgetStatusMessage = isEnabled
            ? $"{definition.Name} enabled. Use Show Widgets to open overlay windows."
            : $"{definition.Name} disabled.";

        if (!result.Success)
        {
            DesktopWidgetStatusMessage = BuildWidgetStatusMessage(result);
        }

        RefreshDesktopWidgetRuntimeStatus();
        return true;
    }

    private void ShowDesktopWidget(DesktopWidgetDefinitionViewModel definition)
    {
        if (!definition.IsImplemented)
        {
            DesktopWidgetStatusMessage = $"{definition.Name} is planned for a future widget stage.";
            return;
        }

        EnableDesktopWidgets = true;
        if (!definition.IsEnabled)
        {
            definition.IsEnabled = true;
        }

        var result = desktopWidgetService.ShowWidgets();
        DesktopWidgetStatusMessage = BuildWidgetStatusMessage(result);
        RefreshDesktopWidgetRuntimeStatus();
    }

    private void RefreshDesktopWidgetRuntimeStatus()
    {
        ActiveDesktopWidgetCount = desktopWidgetService.ActiveWidgetCount;
        DesktopWidgetVisibilityText = desktopWidgetService.IsWidgetsVisible ? "Visible" : "Hidden";
        DesktopWidgetLayoutSummary = desktopWidgetService.GetWidgetLayoutSummary();
    }

    private static string BuildWidgetStatusMessage(WidgetOperationResult result)
    {
        return result.Success
            ? result.Message
            : $"{result.Message}{(string.IsNullOrWhiteSpace(result.Details) ? string.Empty : $" {result.Details}")}";
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

    public void NavigateToCustomizationSection(string sectionKey)
    {
        SelectedCustomizationSection = CustomizationSections
            .FirstOrDefault(section => string.Equals(section.Key, sectionKey, StringComparison.Ordinal))
            ?? SelectedCustomizationSection;
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
