# Strategic spec: T0016 - Context menu + keyboard shortcuts

**Ticket:** T0016
**Proposal:** A7
**Status:** Draft
**Priority:** 45
**Date:** 2026-07-11
**Tier:** Easy
**Tactical plan:** `PLAN/T0016_context-menu-shortcuts/` (created by /spec-tech)

## 1. Problem
Every action requires walking to the button row. There is no right-click context menu on a
scenario and no keyboard shortcuts (Enter to run, Del to remove, F2 to edit), so common
operations are slower than users expect from a list UI.

## 2. Goals
1. Right-clicking a scenario shows a context menu with its actions (Run, Edit, Remove, and any
   of Clone/Export that exist).
2. Keyboard shortcuts on the list: Enter = Run, Del = Remove, F2 = Edit.
3. Shortcuts and menu reuse the existing command handlers - no duplicated logic.

**Non-goals:**
- Global/system-wide hotkeys.
- A configurable keybinding system.

## 4. Current architecture context
`MainWindow` has button-click handlers (`RunButton_Click`, `EditButton_Click`,
`RemoveButton_Click`, and future Clone/Export). A context menu and `InputBindings` should route
to those same handlers/commands against `AppListView.SelectedItem`.

## 5. Proposed approach
Attach a `ContextMenu` to the list items and key `InputBindings` to the list, both dispatching
to the existing handlers. Guard for "no selection" as the buttons already do.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Del key fires without confirmation | Medium | Accidental removal | Reuse the existing remove confirmation dialog |

## 10. Links to other specs
- Depends on which row actions exist: T0007 (double-click), T0008 (export), T0015 (clone).
- Shares the list surface with T0005, T0011, T0022.

## 11. Done criteria (strategic)
1. Right-clicking a scenario shows a working actions menu.
2. Enter runs, F2 edits, and Del removes (with the existing confirmation) the selected scenario.

## 12. Next step
`/spec-tech T0016`.
