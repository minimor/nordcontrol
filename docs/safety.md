# Safety

NordControl should be useful without becoming a fake optimizer or risky tweak bundle. System-level features must be explicit, understandable, logged, and reversible wherever possible.

## Principles

- Future system optimization features must be reversible.
- Do not apply dangerous registry, service, startup, permission, or policy changes without clear explanation.
- Prefer restore points, logs, dry-run previews, and rollback plans before changing system behavior.
- Avoid fake optimizer behavior such as vague cleanup claims, placebo toggles, or one-click mystery fixes.
- Keep Windows-specific operations isolated in `NordControl.Windows`.
- Treat destructive actions as workflows with confirmation, logs, and undo paths.
- Customization features must avoid Explorer patching, system DLL modifications, process injection, background service installation, and risky registry edits.

## Rollback Direction

Future modules should capture the previous state before applying a tweak. For registry edits, store old values. For service changes, store previous startup type and running state. For profile changes, store the prior active profile and every setting changed by the profile.

## Logging Direction

System changes should be logged with enough detail to understand what changed, when it changed, and how to undo it. Logs should avoid secrets and personal data unless the user explicitly opts into more detailed diagnostics.
