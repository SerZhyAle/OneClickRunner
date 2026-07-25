# Spec: T0020 - ConfigurationService logs via LoggingService

**Ticket:** T0020 · **Proposal:** D4 · **Status:** Implemented · **Priority:** 40 · **Tier:** Quick Win · **Date:** 2026-07-11
**Complexity:** Primitive (1 file) - implement directly on approval, no tactical plan.

## Problem
`ConfigurationService` reports load/save errors via `System.Diagnostics.Debug.WriteLine`, which
vanishes in a normal release run and bypasses the app's log file. The project's logging facade is
`LoggingService.Log` (writes to `activity.log`); the CLAUDE.md anti-slop rule forbids non-facade
logging in shipped code.

## Approach
- `OneClickRunner/Services/ConfigurationService.cs` - replace the two `Debug.WriteLine` calls in
  the load and save `catch` blocks with `LoggingService.Log`, keeping the same message content.

## Done criteria
- `ConfigurationService` contains no `Debug.WriteLine`; its error paths write to `activity.log`.
- A forced load/save error is visible in the log file.

## Links
- Pure anti-slop cleanup; independent of other tickets.

**Result (2026-07-11):** Implemented on branch `chore/import-agent-kit`. Release build: 0 errors.
