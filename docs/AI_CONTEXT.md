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
- `NordControl.App/Views/MainWindow.axaml`
- `NordControl.App/Views/Dashboard/DashboardView.axaml`
- `NordControl.App/Views/WindowManager/WindowManagerView.axaml`
- `NordControl.App/Views/Settings/SettingsView.axaml`
- `NordControl.App/Views/Customization/CustomizationView.axaml`
- `NordControl.App/Services/AppStateService.cs`
- `NordControl.Core/Models/AppSettings.cs`
- `NordControl.Core/Models/CustomizationSettings.cs`
- `NordControl.Core/Modules/CustomizationSectionCatalog.cs`
- `NordControl.Core/Modules/CustomizationPresetCatalog.cs`
- `NordControl.Windows/Services/WindowsWindowManagerService.cs`
- `NordControl.Windows/Services/WindowsPersonalizationService.cs`
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
- Customization selected subsection
- NordControl accent color
- Glass-style preview setting
- Low-risk Windows personalization permission

If settings JSON is invalid, the service preserves the broken file as `settings.broken-*.json` and creates defaults.

## Current Limitations

- `MainWindowViewModel` and `MainWindow.axaml` now act as shell/navigation host; module behavior lives in page-specific view models and views.
- Customization taskbar, Start menu, widget, launcher, layout, and Risk Lab sections are planning UI only.
- System accent color and wallpaper changes are not implemented.
- No global hotkeys yet.
- No autopin rules yet.
- No real desktop overlay widgets yet.
- No launcher yet.
- No tiling engine yet.
- No Performance Profiles implementation yet.

## Next Priorities

1. Add theme package models and import/export.
2. Improve app-level theme preview and visual state.
3. Research safe taskbar customization without Explorer patching.
4. Add Desktop Widgets overlay architecture.
5. Add Command Launcher and hotkey support.
6. Add Layout Engine for save/restore and tiling.
7. Keep Risk Lab isolated and explicit.
