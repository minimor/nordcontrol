# Risk Lab

NordControl will eventually explore deeper Windows customization, but risky work must be isolated and explicit.

## Risk Categories

Low risk:

- HKCU personalization registry values
- App-only themes
- App-only preview states
- Overlay windows that can be closed normally

Medium risk:

- Global hotkeys
- Window positioning
- Wallpaper changes
- Startup entries
- Layout restoration

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

## Future Experimental Ideas

- Transparent or blurred taskbar
- Floating taskbar
- Desktop widgets
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
