# Spec: T0007 - Double-click a row to run

**Ticket:** T0007 · **Proposal:** A1 · **Status:** Implemented · **Priority:** 55 · **Tier:** Quick Win · **Date:** 2026-07-11
**Complexity:** Primitive (1-2 files, no new types) - implement directly on approval, no tactical plan.

## Problem
Running a scenario from the settings window requires selecting a row and then clicking **Run**.
Double-clicking a row - the near-universal expectation for a launcher list - does nothing.

## Approach
- `OneClickRunner/MainWindow.xaml` - add a `MouseDoubleClick` handler on `AppListView`.
- `OneClickRunner/MainWindow.xaml.cs` - handler invokes the same code path as
  `RunButton_Click` for the double-clicked item (reuse, do not duplicate the launch logic;
  ideally the shared launcher from T0001).

## Done criteria
- Double-clicking a scenario row starts it, identically to selecting it and clicking Run.
- Double-clicking empty space does nothing.

## Links
- Reuse the launch path from T0001; complements T0016 (Enter-to-run shortcut).

**Result (2026-07-11):** Implemented on branch `chore/import-agent-kit`. Release build: 0 errors.
