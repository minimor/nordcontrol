# NordControl

NordControl is a personal Windows desktop control center built with C# and Avalonia. The goal is to grow it into a full desktop utility with a clean architecture, a dark modern shell, and safe Windows-specific modules that can evolve over time.

The first milestone is intentionally conservative: project structure, application shell, navigation placeholders, documentation, and test coverage for non-UI basics.

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

## Planned Modules

- Dashboard
- Window Manager
- Performance Profiles
- Windows Customization
- Settings
- Logging
- Safe system tweaks with rollback in future versions
