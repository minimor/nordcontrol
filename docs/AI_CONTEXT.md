# AI Context

## What NordControl Is

NordControl is a personal Windows desktop control center built with C#, .NET, and Avalonia. It is intended to grow into a safe, modern Windows customization/control center with Window Manager, Settings, Customization Studio, Dashboard, and future desktop environment features.

The project is inspired conceptually by desktop customization tools such as Seelen UI, PowerToys-like utilities, launchers, widgets, and tiling/window management tools. NordControl must implement its own architecture and assets independently.

## Current Branch

The active development branch is:

```text
dev
```

Use `dev` for normal work. Do not create new feature branches unless the user explicitly requests one.

## Previous Development Chain

- `feature/project-bootstrap`
- `feature/window-pin-manager`
- `feature/settings-storage`
- `feature/window-auto-refresh`
- `feature/customization-studio`
- `feature/customization-sections`
- `dev`

## Current App State

The app currently includes:

- Avalonia shell with global left navigation and module page hosting.
- Dashboard with open/topmost window counts.
- Window Manager with visible window enumeration, search, refresh, topmost pin/unpin, and auto-refresh.
- Persistent app settings.
- Settings page.
- Customization Studio.
- Theme Package system with built-in packages, user/imported packages, JSON import/export, custom editing, and live preview.
- Customization internal sections:
  - Overview
  - Themes
  - Taskbar
  - Start Menu
  - Desktop Widgets
  - Window Effects
  - Launcher
  - Layouts / Tiling
  - Advanced / Risk Lab
- Windows personalization safe controls for current-user apps/system theme, transparency, and title-bar accent.
- Preset gallery and desktop environment roadmap cards.
- Theme package gallery, import/export controls, custom theme editor, and live preview in the Customization Themes subsection.
- Taskbar Lab V1 in the Customization Taskbar subsection, with read-only current-user taskbar state detection, preview-only taskbar presets, safety toggles, and an app-only live taskbar mockup.

## Existing Modules

- Dashboard
- Window Manager
- Performance Profiles placeholder
- Customization
- Settings

## Important Files

- `NordControl.App/ViewModels/Shell/MainWindowViewModel.cs`
- `NordControl.App/ViewModels/Shell/ShellModuleViewModel.cs`
- `NordControl.App/ViewModels/Dashboard/DashboardViewModel.cs`
- `NordControl.App/ViewModels/WindowManager/WindowManagerViewModel.cs`
- `NordControl.App/ViewModels/Settings/SettingsViewModel.cs`
- `NordControl.App/ViewModels/Customization/CustomizationViewModel.cs`
- `NordControl.App/ViewModels/Customization/ThemePackageViewModel.cs`
- `NordControl.App/Views/MainWindow.axaml`
- `NordControl.App/Views/Dashboard/DashboardView.axaml`
- `NordControl.App/Views/WindowManager/WindowManagerView.axaml`
- `NordControl.App/Views/Settings/SettingsView.axaml`
- `NordControl.App/Views/Customization/CustomizationView.axaml`
- `NordControl.App/Services/AppStateService.cs`
- `NordControl.Core/Models/AppSettings.cs`
- `NordControl.Core/Models/CustomizationSettings.cs`
- `NordControl.Core/Models/ThemePackage.cs`
- `NordControl.Core/Models/TaskbarSettings.cs`
- `NordControl.Core/Models/TaskbarState.cs`
- `NordControl.Core/Models/TaskbarPreset.cs`
- `NordControl.Core/Modules/TaskbarPresetCatalog.cs`
- `NordControl.Core/Services/ITaskbarService.cs`
- `NordControl.Core/Modules/ThemePackageCatalog.cs`
- `NordControl.Core/Services/IThemePackageService.cs`
- `NordControl.Core/Services/JsonThemePackageService.cs`
- `NordControl.Core/Modules/CustomizationSectionCatalog.cs`
- `NordControl.Core/Modules/CustomizationPresetCatalog.cs`
- `NordControl.Windows/Services/WindowsWindowManagerService.cs`
- `NordControl.Windows/Services/WindowsPersonalizationService.cs`
- `NordControl.Windows/Services/WindowsTaskbarService.cs`
- `NordControl.Windows/Services/JsonAppSettingsService.cs`

## Settings

Settings use `AppSettings` and are persisted by `JsonAppSettingsService`.

Settings file path:

```text
%AppData%\NordControl\settings.json
```

Current settings include:

- Theme
- Last selected global module
- Window Manager settings
- Customization selected preset
- Customization selected theme package
- Last exported theme package path
- Last imported theme package path
- Whether theme packages should update the NordControl shell preview
- Customization selected subsection
- NordControl accent color
- Glass-style preview setting
- Low-risk Windows personalization permission
- Taskbar Lab settings:
  - selected taskbar preset key
  - enable Taskbar Lab
  - preview-only mode
  - medium-risk taskbar change permission
  - warning visibility
  - last applied time

If settings JSON is invalid, the service preserves the broken file as `settings.broken-*.json` and creates defaults.

## Current Limitations

- `MainWindowViewModel` and `MainWindow.axaml` now act as shell/navigation host; module behavior lives in page-specific view models and views.
- Customization Start menu, widget, launcher, layout, and Risk Lab sections are planning UI only.
- Taskbar Lab V1 is preview-first. It does not replace the real taskbar, hook Explorer, patch Explorer, restart Explorer, or implement real blur/acrylic taskbar effects.
- Theme package import uses a path textbox rather than a native file picker.
- System accent color and wallpaper changes are not implemented.
- No global hotkeys yet.
- No autopin rules yet.
- No real desktop overlay widgets yet.
- No launcher yet.
- No tiling engine yet.
- No Performance Profiles implementation yet.

## Next Priorities

1. Improve app-level dynamic styling beyond the Customization live preview.
2. Add native file picker support for theme import/export.
3. Expand Taskbar Lab only with safe, reversible changes; real taskbar blur/replacement remains future Risk Lab work.
4. Add Desktop Widgets overlay architecture.
5. Add Command Launcher and hotkey support.
6. Add Layout Engine for save/restore and tiling.
7. Keep Risk Lab isolated and explicit.
