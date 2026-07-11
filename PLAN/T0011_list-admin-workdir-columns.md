# Spec: T0011 - Show RunAsAdmin + Working Dir in the list

**Ticket:** T0011 · **Proposal:** A2 · **Status:** Draft · **Priority:** 50 · **Tier:** Easy · **Date:** 2026-07-11
**Complexity:** Primitive (1-2 files, no new types) - implement directly on approval, no tactical plan.

## Problem
The scenario list shows only Name, Path, and Arguments. Whether a scenario runs elevated
(`RunAsAdmin`) or has a Working Directory is invisible, so the user cannot tell at a glance
which scenarios will trigger UAC - especially relevant once launch elevation is consistent
(T0001).

## Approach
- `OneClickRunner/MainWindow.xaml` - add a column for `RunAsAdmin` (a shield glyph or a
  checkmark bound to the bool) and, optionally, a Working Directory column.
- Keep binding to the existing `AppItem` properties; no model change.

## Done criteria
- A scenario with `RunAsAdmin = true` is visibly marked as admin in the list.
- The Working Directory (when set) is visible or discoverable in the list row.

## Links
- Complements the elevation fix T0001; shares the list surface with T0005, T0016, T0022.
