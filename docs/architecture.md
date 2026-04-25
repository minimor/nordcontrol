# Architecture

NordControl is split into separate projects so the app can grow without mixing UI, business contracts, and Windows-specific implementation details.

## Project Roles

`NordControl.App` contains the Avalonia desktop application. Views, view models, application startup, styling, and navigation shell code belong here. It can reference Core contracts and Windows service implementations, but Avalonia views should not directly contain WinAPI, registry, or system-tweak logic.

`NordControl.Core` contains shared models, interfaces, contracts, module definitions, and app-level abstractions. It should stay free of Avalonia dependencies and direct WinAPI calls. Code in this project should be easy to test and safe to reuse from other layers.

`NordControl.Windows` contains Windows-specific implementations. Future P/Invoke, WinAPI wrappers, registry access, service control, window enumeration, and system integrations belong here. This keeps platform concerns behind interfaces defined in Core.

`NordControl.Tests` contains tests for non-UI logic. Core behavior should be tested here first. Windows services can also be tested when they expose deterministic behavior or safe environment reads.

## Dependency Direction

- `NordControl.App` references `NordControl.Core` and `NordControl.Windows`.
- `NordControl.Windows` references `NordControl.Core`.
- `NordControl.Tests` references `NordControl.Core` and may reference `NordControl.Windows`.
- `NordControl.Core` does not reference App or Windows.

This keeps contracts and models at the center, with UI and platform code depending inward.

## WinAPI Boundary

All future WinAPI and P/Invoke code should live in `NordControl.Windows`. The app can ask for window-management behavior through Core interfaces, but implementation details should remain in Windows services.

## UI Boundary

Avalonia XAML, view models, navigation, and visual state belong in `NordControl.App`. UI code should coordinate services rather than perform low-level system operations directly.

## Models and Interfaces

Shared models and service interfaces belong in `NordControl.Core`. Examples include module definitions, settings contracts, profile models, logging abstractions, and safe operation results.
