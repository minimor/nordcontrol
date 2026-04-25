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

Status: In progress. The Theme Package model, built-in catalog, JSON export service, settings persistence, and Customization Themes package gallery are implemented.

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

Deliverables:

- Safe taskbar transparency research
- Preview-only controls
- Optional low-risk registry/API experiments
- Explicit risk labels
- No Explorer restart

Risk level: Medium

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 4: Desktop Widgets Overlay

Goal: Add a safe overlay window architecture for widgets.

Deliverables:

- Separate always-on-top transparent widget windows
- Clock widget placeholder
- System monitor widget placeholder
- Music controls placeholder
- Widget settings model

Risk level: Medium

Branch/commit note: Commit to `dev` unless asked otherwise.

## Stage 5: Command Launcher

Goal: Build a launcher and command palette for NordControl and app search.

Deliverables:

- Global hotkey support
- App search
- Command palette
- NordControl actions
- Conflict/disable settings

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
