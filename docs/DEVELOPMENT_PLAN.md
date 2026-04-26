# Development Plan

All normal work should happen on `dev` unless the user explicitly requests another branch.

## Stage 0: Workflow And Context

Goal: Consolidate the project so future AI/developer sessions can continue safely.

Deliverables:

- `dev` branch workflow
- AI context docs
- Architecture cleanup notes
- Risk Lab boundaries

Risk level: Low

Branch/commit note: Commit to `dev`.

## Stage 1: Navigation And MVVM Split

Goal: Split the current large `MainWindowViewModel` and `MainWindow.axaml` into real pages and view models.

Status: Implemented. `MainWindow` is now a shell/navigation host with Dashboard, Window Manager, Settings, and Customization page view models and views.

Deliverables:

- Navigation service or shell coordinator
- Dashboard view/view model
- Window Manager view/view model
- Settings view/view model
- Customization view/view model
- Existing behavior preserved

Risk level: Low to Medium

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 2: Customization Studio Architecture

Goal: Turn Customization Studio into a real theme/configuration system.

Status: In progress. Theme packages now support built-in and user packages, JSON import/export, persistent AppData theme storage, a custom theme editor, settings persistence, and a live app preview in Customization.

Deliverables:

- Theme package model
- Import/export theme JSON
- App-level theme preview
- Preset editing foundation
- Safer settings persistence around customization

Risk level: Low

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 3: Taskbar Lab V1

Goal: Research safe taskbar customization without shell patching.

Status: Implemented as a preview-first foundation. Taskbar Lab V1 now lives inside Customization -> Taskbar with persisted taskbar settings, a service abstraction, a Windows read-only taskbar snapshot service, preview-only presets, safety toggles, and an app-only live taskbar mockup.

Deliverables:

- Taskbar settings model
- Taskbar service abstraction
- Safe/read-only taskbar state detection for known current-user values
- Preview-only taskbar presets
- Explicit risk labels and safety toggles
- Live app-only taskbar preview
- No Explorer restart, hooks, injection, replacement, services, or shell patching

Risk level: Medium

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 4: Desktop Widgets Overlay

Goal: Add a safe overlay window architecture for widgets.

Status: Implemented and improved. Desktop Widgets Overlay V2 now lives inside Customization -> Desktop Widgets with persisted widget settings, a definition catalog, app-owned Avalonia overlay windows, manual show/hide/reset controls, draggable/resizable Clock and System Monitor Lite widgets, saved positions and sizes, lock/unlock controls, planned widget cards, runtime status, and a live preview.

Deliverables:

- Separate app-owned overlay widget windows
- Draggable and resizable widgets when unlocked
- Saved widget positions and sizes
- Screen-safe bounds normalization and reset layout defaults
- Improved Clock widget
- System Monitor Lite widget with app CPU, app memory, and GC memory budget values
- Planned Music Controls, Quick Notes, Shortcuts Panel, Weather, and Performance Monitor definitions
- Widget settings model and normalization
- Manual show/hide/reset lifecycle controls
- Optional start-with-app setting
- No Explorer patching, shell injection, services, global hooks, or desktop replacement

Risk level: Medium

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 5: Command Launcher

Goal: Build a launcher and command palette for NordControl and app search.

Status: Implemented as a V1 foundation. Command Launcher V1 includes a glass-style overlay, search, keyboard navigation, built-in NordControl commands, module navigation, Customization subsection navigation, widget commands, theme commands, safe system actions, Start Menu shortcut app search, Settings integration, a visible shell Launcher button, and a local `Ctrl+Space` shortcut while NordControl is focused.

Deliverables:

- Launcher settings model
- Launcher command catalog and search scoring
- App-owned launcher overlay window
- Keyboard navigation with Enter/Escape/Up/Down
- NordControl navigation and safe action routing
- Desktop widget commands
- Theme package commands
- Start Menu `.lnk` app search and launch
- Settings integration
- Local Ctrl+Space shortcut and launcher button
- Native global hotkey service path documented as planned; no Explorer patching, shell injection, service installation, or shell replacement

Risk level: Medium

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 6: Layout Engine / Tiling

Goal: Add safe window layout and tiling tools.

Deliverables:

- Save/restore window layouts
- Zones
- Simple tiling using `SetWindowPos`
- Per-app rules
- Rollback/restore behavior

Risk level: Medium

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 7: Media Module

Goal: Add media status and controls.

Deliverables:

- Windows media session integration if practical
- Current track display
- Basic media controls
- Dashboard/widget integration

Risk level: Low to Medium

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 8: Risk Lab

Goal: Isolate high-risk experiments so they cannot accidentally become normal app behavior.

Deliverables:

- Experimental taskbar replacement concepts
- Start menu experiments
- Custom Alt+Tab research
- Virtual desktop viewer
- Explorer integration research
- Explicit warnings, confirmations, logs, and rollback plans

Risk level: High

Branch/commit note: Commit to `dev` only when explicitly requested. Consider separate branches only if the user asks.
