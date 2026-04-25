# NordControl

NordControl is a personal Windows desktop control center built with C# and Avalonia. The goal is to grow it into a full desktop utility with a clean architecture, a dark modern shell, and safe Windows-specific modules that can evolve over time.

The long-term direction is a Seelen UI-inspired Windows customization and desktop environment lab implemented independently in C# and Avalonia. NordControl should take conceptual inspiration from modern customization tools without copying code, assets, branding, or implementation details.

## Tech Stack

- C#
- .NET 10.0 in this environment, compatible with the requirement for .NET 8 or newer stable SDKs
- Avalonia UI
- MVVM with CommunityToolkit.Mvvm
- xUnit for tests

## Solution Structure

- `NordControl.App` - Avalonia desktop UI, views, view models, startup, and navigation shell.
- `NordControl.Core` - Shared models, interfaces, contracts, and app-level abstractions.
- `NordControl.Windows` - Windows-specific service implementations and future WinAPI or registry integration points.
- `NordControl.Tests` - Tests for non-UI logic and platform service basics.

## Branch Workflow

- `dev` is the active development branch.
- `main` is stable/manual-merge only.
- Historical feature branches should not be used for new work unless explicitly requested.
- Normal tasks should commit and push to `dev`.

## Build

```powershell
dotnet restore .\NordControl.sln
dotnet build .\NordControl.sln
```

## Run

```powershell
dotnet run --project .\NordControl.App
```

## Test

```powershell
dotnet test .\NordControl.sln
```

## Current Status

Stage 1 is in progress. The repository contains a working Avalonia app shell with dark styling, sidebar navigation, Dashboard window counts, a Window Manager page, native topmost pin/unpin support, persistent JSON settings under AppData, a subsection-based Customization Studio foundation, initial Core abstractions, a Windows service layer, and basic tests.

Customization Studio is now the foundation for the future Desktop Environment Lab. It includes safe Windows personalization controls, NordControl presets, internal sections, and roadmap cards for Taskbar, Start Menu, Desktop Widgets, Window Effects, Launcher, Layouts / Tiling, and Advanced / Risk Lab.

See:

- `docs/AI_CONTEXT.md`
- `docs/DEVELOPMENT_PLAN.md`
- `docs/SEELEN_UI_STUDY.md`
- `docs/RISK_LAB.md`
- `docs/BRANCH_WORKFLOW.md`

## Planned Modules

- Dashboard
- Window Manager
- Performance Profiles
- Windows Customization
- Settings
- Logging
- Safe system tweaks with rollback in future versions
