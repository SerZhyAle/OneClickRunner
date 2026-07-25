# Spec: T0018 - Allow zero scenarios (no forced Calculator reseed)

**Ticket:** T0018 · **Proposal:** B5 · **Status:** Implemented · **Priority:** 40 · **Tier:** Quick Win · **Date:** 2026-07-11
**Complexity:** Primitive (1 file, small logic) - implement directly on approval, no tactical plan.

## Problem
`ConfigurationService.LoadConfiguration` re-seeds a default "Calculator" scenario **every** time
the Scenarios folder is empty. A user who deliberately removes all scenarios gets Calculator
back on the next load, and cannot keep an empty list.

## Approach
- `OneClickRunner/Services/ConfigurationService.cs` - seed the default only on genuine **first
  run** (e.g. a first-run marker), not on every empty-folder load, so an intentionally empty list
  stays empty.
- Preserve the current friendly first-run experience (a new user still gets the sample).

## Done criteria
- Removing all scenarios and reopening leaves the list empty (no Calculator reappears).
- A brand-new install with no prior state still gets the sample Calculator once.

## Links
- Touches the same loader as T0005 (ordering) - coordinate if both are picked up.

**Result (2026-07-11):** Implemented on branch `chore/import-agent-kit`. Release build: 0 errors.
