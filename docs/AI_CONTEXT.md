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
- Desktop Widgets Overlay V2 in the Customization Desktop Widgets subsection, with persisted widget settings, an app-owned Avalonia overlay window controller, draggable/resizable Clock and System Monitor Lite overlay widgets, saved positions/sizes, lock/unlock controls, planned widget definitions, and a live preview.
- Command Launcher V1 with a glass-style overlay window, command search, keyboard navigation, NordControl navigation/actions, widget commands, theme commands, safe system actions, Start Menu `.lnk` app search, a shell Launcher button, and a local `Ctrl+Space` shortcut while NordControl is focused.

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
- `NordControl.Core/Models/DesktopWidgetSettings.cs`
- `NordControl.Core/Models/DesktopWidgetInstanceSettings.cs`
- `NordControl.Core/Models/DesktopWidgetDefinition.cs`
- `NordControl.Core/Modules/TaskbarPresetCatalog.cs`
- `NordControl.Core/Modules/DesktopWidgetCatalog.cs`
- `NordControl.Core/Services/ITaskbarService.cs`
- `NordControl.Core/Services/IDesktopWidgetService.cs`
- `NordControl.Core/Models/LauncherSettings.cs`
- `NordControl.Core/Models/LauncherCommand.cs`
- `NordControl.Core/Modules/LauncherCommandCatalog.cs`
- `NordControl.Core/Modules/LauncherSearchEngine.cs`
- `NordControl.Core/Services/ILauncherService.cs`
- `NordControl.Core/Modules/ThemePackageCatalog.cs`
- `NordControl.Core/Services/IThemePackageService.cs`
- `NordControl.Core/Services/JsonThemePackageService.cs`
- `NordControl.Core/Modules/CustomizationSectionCatalog.cs`
- `NordControl.Core/Modules/CustomizationPresetCatalog.cs`
- `NordControl.Windows/Services/WindowsWindowManagerService.cs`
- `NordControl.Windows/Services/WindowsPersonalizationService.cs`
- `NordControl.Windows/Services/WindowsTaskbarService.cs`
- `NordControl.Windows/Services/JsonAppSettingsService.cs`
- `NordControl.App/Services/DesktopWidgetService.cs`
- `NordControl.App/Views/Widgets/DesktopWidgetWindow.axaml`
- `NordControl.App/Views/Widgets/ClockWidgetView.axaml`
- `NordControl.App/Views/Widgets/SystemMonitorLiteWidgetView.axaml`
- `NordControl.App/Services/LauncherService.cs`
- `NordControl.App/Services/LauncherWindowService.cs`
- `NordControl.App/Views/Launcher/LauncherWindow.axaml`

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
- Desktop Widget settings:
  - enable widgets
  - start widgets with app
  - lock widget positions
  - show widget background
  - global opacity
  - selected widget theme key
  - widget instance positions, sizes, enabled state, opacity, and always-on-top preference
- Launcher settings:
  - enable launcher
  - start with app
  - hotkey gesture
  - show on startup
  - include NordControl commands
  - include apps
  - include system actions
  - close after action
  - max results

If settings JSON is invalid, the service preserves the broken file as `settings.broken-*.json` and creates defaults.

## Current Limitations

- `MainWindowViewModel` and `MainWindow.axaml` now act as shell/navigation host; module behavior lives in page-specific view models and views.
- Customization Start menu, launcher, layout, and Risk Lab sections are planning UI only.
- Taskbar Lab V1 is preview-first. It does not replace the real taskbar, hook Explorer, patch Explorer, restart Explorer, or implement real blur/acrylic taskbar effects.
- Desktop Widgets Overlay V2 uses app-owned Avalonia windows only. It does not replace the desktop, integrate with Explorer, install services, or use global hooks. Widgets can be dragged/resized while unlocked and persist their bounds. System Monitor Lite currently reports app CPU and app memory/GC memory budget; broader system-wide CPU/RAM telemetry is future work.
- Command Launcher V1 uses an app-owned overlay and local `Ctrl+Space` shortcut while NordControl is focused. Native global hotkey registration is not active yet; the service reports this limitation instead of crashing.
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
4. Improve Desktop Widgets with richer lightweight system metrics and additional widget types.
5. Add native global hotkey registration for the Command Launcher after isolating the Win32 message pump.
6. Add Layout Engine for save/restore and tiling.
7. Keep Risk Lab isolated and explicit.
