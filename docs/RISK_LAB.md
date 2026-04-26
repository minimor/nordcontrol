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

## Future Experimental Ideas

- Transparent or blurred taskbar
- Floating taskbar
- Real taskbar blur/acrylic implementation
- Taskbar replacement or Explorer-integrated taskbar modules
- Desktop widgets
- Widget dragging and saved overlay positions
- Rich system metrics widgets
- Launcher
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

## Desktop Widgets Overlay V1 Boundary

Desktop Widgets Overlay V1 is implemented as app-owned Avalonia windows controlled by NordControl. V1 includes Clock and System Monitor Lite widgets, manual show/hide/reset controls, persisted settings, and preview UI inside Customization -> Desktop Widgets.

V1 does not patch Explorer, inject into the shell, replace the desktop, install services, add global hooks, create shell extensions, or modify registry keys. Dragging with saved positions and richer live system metrics are future improvements.
