# Seelen UI Concept Study

## Inspiration Summary

Seelen UI is useful as conceptual inspiration for NordControl because it treats Windows customization as a broader desktop environment experience rather than a collection of isolated tweaks.

The inspiration is about product direction and UX ideas, not code reuse.

## General Concepts

Concepts worth studying:

- Customizable Windows desktop environment
- Themes
- Taskbar layout ideas
- Widgets
- App launcher
- Tiling window manager
- Media module
- Settings-driven customization
- Advanced switching and virtual desktops as future ideas

## Different Stack

Seelen UI uses a different implementation stack, including:

- Tauri
- Rust
- Svelte / Preact
- TypeScript

NordControl uses:

- C#
- .NET
- Avalonia
- WinAPI/PInvoke where needed
- Windows service layer in `NordControl.Windows`

## Licensing Caution

Seelen UI is AGPL-licensed. NordControl must not copy Seelen UI source code, assets, branding, visual assets, exact implementation, or internal architecture.

NordControl should implement similar categories of ideas independently in C# and Avalonia, with its own UX, models, services, and safety gates.

## Proposed NordControl Equivalents

- Seelen themes -> NordControl Theme Packages
- Seelen toolbar/taskbar concepts -> NordControl Taskbar Lab
- Seelen widgets -> NordControl Desktop Widgets overlay
- Seelen launcher -> NordControl Command Launcher
- Seelen tiling -> NordControl Layout Engine
- Seelen media module -> NordControl Media Module
- Seelen settings -> NordControl Customization Studio

## NordControl Theme Package Notes

NordControl theme packages are an independent C# model for app and future overlay visuals. They describe colors, glass behavior, corner radius, mood, and tags, and can be exported as NordControl JSON files. They are conceptual equivalents only; do not copy Seelen UI package formats, assets, code, naming, or branding.

## Risks And Technical Challenges

- Explorer/taskbar integration can break after Windows updates.
- Replacing or deeply modifying shell behavior is high risk.
- Global hooks and process injection require strict isolation and explicit opt-in.
- Overlay windows must avoid interfering with input, focus, and accessibility.
- Global hotkeys need conflict detection and user control.
- Tiling and layout restoration must handle disappearing windows and privilege boundaries.
- Media integration may depend on Windows APIs that vary by app and OS version.
- Any risky feature needs logs, rollback paths, and clear enable/disable settings.
