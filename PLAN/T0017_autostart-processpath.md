# Spec: T0017 - Autostart via Environment.ProcessPath

**Ticket:** T0017 · **Proposal:** C4 · **Status:** Implemented · **Priority:** 45 · **Tier:** Quick Win · **Date:** 2026-07-11
**Complexity:** Primitive (1 file, few lines) - implement directly on approval, no tactical plan.

## Problem
`AutostartService.EnableAutostart` derives the executable path from
`Assembly.GetExecutingAssembly().Location` and then string-replaces `.dll` with `.exe`. Under a
single-file publish `Location` can be empty, producing a broken or empty Run-key value, so
autostart silently fails to launch the app.

## Approach
- `OneClickRunner/Services/AutostartService.cs` - use `Environment.ProcessPath` (.NET 6+) as the
  executable path instead of the `Assembly.Location` + `.dll`->`.exe` trick. Keep the registry
  key and value name unchanged.

## Done criteria
- The Run-key value points at the real running `.exe` for both a normal build and a single-file
  publish.
- Enable/disable autostart still toggles the same `HKCU\...\Run` value.

## Links
- Independent; touches only AutostartService.

**Result (2026-07-11):** Implemented on branch `chore/import-agent-kit`. Release build: 0 errors.
