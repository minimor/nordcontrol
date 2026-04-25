# Branch Workflow

## Branch Roles

- `main`: stable branch, updated only by manual merge when the user decides.
- `dev`: active working branch for normal Codex development.
- Old feature branches: historical context only. Do not use them for new work unless the user explicitly asks.

## Codex Rules

- Start from `dev`.
- Commit to `dev` after every completed prompt.
- Push `dev` after every completed prompt.
- Do not work directly on `main`.
- Do not create a new feature branch unless explicitly requested.
- Do not create a PR unless explicitly requested.
- Do not merge branches unless explicitly requested.
- Do not force push unless explicitly approved.
- Do not delete old branches in routine tasks.

## Expected End Of Task Flow

```powershell
dotnet restore .\NordControl.sln
dotnet build .\NordControl.sln --no-restore
dotnet test .\NordControl.sln --no-build
git status
git branch --show-current
git log --oneline --max-count=8
git add .
git commit -m "<task summary>"
git push -u origin dev
```

If push fails, report the exact command, exact error, and shortest fix.
