# Spec: T0015 - Clone a scenario

**Ticket:** T0015 · **Proposal:** A5 · **Status:** Implemented · **Priority:** 45 · **Tier:** Quick Win · **Date:** 2026-07-11
**Complexity:** Primitive (1-2 files, reuses AddItem) - implement directly on approval, no tactical plan.

## Problem
Making a variant of an existing scenario means re-entering every field by hand. There is no
"duplicate".

## Approach
- `OneClickRunner/MainWindow.xaml` - add a **Clone** button.
- `OneClickRunner/MainWindow.xaml.cs` - copy the selected `AppItem` into a new one with a fresh
  `Id`/`Filename` (and a "(copy)" name), persist via the existing `ConfigurationService.AddItem`,
  then refresh - reusing the Import pattern that already assigns a new Id.

## Done criteria
- Cloning a scenario creates an independent copy with its own Id that can be edited without
  touching the original.
- The Jump List refreshes to include the clone.

## Links
- Reuses the fresh-Id logic from Import; interacts with ordering (T0005: clone appends).

**Result (2026-07-11):** Implemented on branch `chore/import-agent-kit`. Release build: 0 errors.
