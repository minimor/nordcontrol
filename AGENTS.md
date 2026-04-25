# NordControl Agent Guide

## Project Summary

NordControl is a personal Windows desktop control center built with C# and Avalonia. It is evolving into a safe, settings-driven Windows customization and control app with modules for Dashboard, Window Manager, Settings, Customization Studio, and future desktop environment features.

NordControl is conceptually inspired by modern customization tools and desktop environment projects, but it must be implemented independently. Do not copy code, assets, branding, or implementation details from Seelen UI or any other project.

## Tech Stack

- C#
- .NET 10 in the current environment, with .NET 8 or newer as the intended baseline
- Avalonia UI
- MVVM with CommunityToolkit.Mvvm
- xUnit tests
- Windows-specific functionality isolated behind service interfaces

## Branch Workflow

- Use `dev` as the active working branch.
- Do not work directly on `main`.
- Do not create new feature branches unless the user explicitly asks.
- Commit every completed task to `dev`.
- Push `dev` after every completed task.
- Do not merge into `main` unless explicitly asked.
- Do not create pull requests unless explicitly asked.
- Do not rewrite history.
- Do not delete historical feature branches unless explicitly asked.

## Commands

Restore:

```powershell
dotnet restore .\NordControl.sln
```

Build:

```powershell
dotnet build .\NordControl.sln --no-restore
```

Test:

```powershell
dotnet test .\NordControl.sln --no-build
```

Run:

```powershell
dotnet run --project .\NordControl.App
```

## Architecture Rules

- `NordControl.App`: Avalonia UI, views, view models, binding, app startup, user interaction.
- `NordControl.Core`: models, interfaces, contracts, pure logic, catalogs, validation helpers. No Avalonia dependencies. No WinAPI/PInvoke.
- `NordControl.Windows`: Windows-specific services, WinAPI, PInvoke, registry, shell integration, AppData path handling.
- `NordControl.Tests`: tests for Core logic and safe service behavior. Avoid brittle tests that modify real Windows settings.

Keep platform operations behind interfaces defined in Core. Do not put PInvoke, registry edits, or shell integration directly in Avalonia views or view models.

## Safety Rules

- Do not patch system DLLs without an explicit Risk Lab task.
- Do not inject into Explorer without an explicit Risk Lab task.
- Do not install services without an explicit task.
- Do not restart Explorer automatically.
- Do not modify risky registry keys silently.
- Do not add startup entries unless explicitly requested.
- Do not add global shell hooks unless explicitly requested.
- All risky features must be isolated, reversible where possible, logged, and gated behind explicit user confirmation.

## Commit And Push Requirements

After each completed task:

1. Run restore, build, and tests.
2. Run an app smoke test when practical.
3. Show `git status`, current branch, and recent log.
4. Commit to `dev` with a clear message.
5. Push `dev` to GitHub.

## What Not To Do

- Do not merge into `main`.
- Do not create PRs unless explicitly asked.
- Do not force-push unless the user explicitly approves and the reason is clear.
- Do not regenerate the solution or recreate existing projects unless explicitly asked.
- Do not implement dangerous Windows customization as a side effect of UI or planning work.
