# Spec: T0004 - Validate scenario path before launch

**Ticket:** T0004 · **Proposal:** B3 · **Status:** Implemented · **Priority:** 70 · **Tier:** Easy · **Date:** 2026-07-11
**Complexity:** Primitive (shared launcher path, no new types) - implement directly on approval, no tactical plan.

## Problem
When a scenario's `Path` does not exist or is not launchable, `Process.Start` throws. From the
Jump List this is only written to `activity.log` and the user sees nothing; from the Run button
it surfaces as a raw exception message. There is no upfront, friendly check.

## Approach
- In the shared launcher (T0001) - or both current call sites until then - check the target is
  resolvable before starting: an existing file, or a bare command resolvable on `PATH` (e.g.
  `calc.exe`). On failure, log and surface a clear message; from the Jump List, defer the
  user-visible part to the notification in T0006.
- Do not block the special/sentinel scenarios (yt-dlp) from their own handling.

## Done criteria
- Launching a scenario whose `Path` points at a missing file produces a clear, logged message
  and no unhandled exception.
- A valid `PATH`-resolved command (`calc.exe`) still launches.

## Links
- Best implemented on top of T0001; user-facing surfacing for Jump List via T0006.

**Result (2026-07-11):** Implemented in `ScenarioLauncher.TryResolveTarget`, run before every
executable launch. Passes for an existing file, an http/https URL, a file relative to the working
directory, or a bare command resolvable on PATH/PATHEXT; otherwise returns a clear message
("File not found: …" / "'…' was not found as a file or on PATH."). No unhandled exception reaches the
caller. yt-dlp scenarios bypass this and use their own discovery check. Jump List surfacing lands in
T0006. Release build: 0 errors.
