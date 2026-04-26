# Risk Lab

NordControl will eventually explore deeper Windows customization, but risky work must be isolated and explicit.

## Risk Categories

Low risk:

- HKCU personalization registry values
- App-only themes
- App-only preview states
- Overlay windows that can be closed normally
- App-owned desktop widget overlay windows

Medium risk:

- Global hotkeys
- Window positioning
- Wallpaper changes
- Startup entries
- Layout restoration
- Future opt-in taskbar setting writes that are reversible and clearly explained

High risk:

- Explorer/taskbar integration
- Taskbar replacement
- Start menu replacement
- Shell hooks
- Process injection
- Custom Alt+Tab
- Virtual desktop integration

## Rules

- Risky features must be isolated.
- Every risky feature needs a clear enable/disable setting.
- Every risky feature needs a rollback strategy where practical.
- Log actions that change system behavior.
- Do not make silent changes.
- Do not restart Explorer automatically.
- Require user confirmation before applying risky behavior.
- Prefer preview-only UI until the implementation is understood.
- Keep Taskbar Lab V1 app-only except for read-only current-user state detection.
- Keep Desktop Widgets Overlay V1 app-owned and hideable from NordControl.
- Keep Command Launcher app-owned; global hotkey registration must be reversible and must not hook Explorer.

## Future Experimental Ideas

- Transparent or blurred taskbar
- Floating taskbar
- Real taskbar blur/acrylic implementation
- Taskbar replacement or Explorer-integrated taskbar modules
- Desktop widgets
- Rich system metrics widgets
- Launcher
- Native global hotkey registration for the launcher
- Tiling manager
- Custom Alt+Tab
- Virtual desktop viewer
- Start menu experiments
- Explorer integration research

## Explicit Non-Goals Without User Approval

- No DLL patching
- No Explorer injection
- No background service installation
- No forced shell restarts
- No hidden registry changes

## Taskbar Lab V1 Boundary

Taskbar Lab V1 is implemented as a safe foundation in Customization -> Taskbar. It includes persisted settings, read-only taskbar state detection, preview-only presets, and an app-only visual mockup.

V1 does not patch Explorer, inject into Explorer, install shell hooks, replace the real taskbar, restart Explorer, or implement real taskbar blur/acrylic. Those ideas remain future Risk Lab work and require explicit confirmation, logging, and rollback planning before any implementation.

## Desktop Widgets Overlay V2 Boundary

Desktop Widgets Overlay V2 is implemented as app-owned Avalonia windows controlled by NordControl. V2 includes draggable/resizable Clock and System Monitor Lite widgets, saved positions and sizes, manual show/hide/reset controls, persisted settings, lock/unlock behavior, and preview/status UI inside Customization -> Desktop Widgets.

V2 does not patch Explorer, inject into the shell, replace the desktop, install services, add global hooks, create shell extensions, or modify registry keys. System Monitor Lite reports safe app-owned metrics for now; richer system-wide metrics remain future work and should stay lightweight and reversible.

## Command Launcher V1 Boundary

Command Launcher V1 is implemented as an app-owned Avalonia overlay with command search, keyboard navigation, NordControl actions, widget controls, safe system actions, and Start Menu shortcut app search.

V1 does not patch Explorer, inject into the shell, replace Start menu, replace the taskbar, install services, or create shell extensions. Native global hotkey registration is planned but not active in V1; `Ctrl+Space` works while NordControl is focused and the visible Launcher button opens the overlay.
