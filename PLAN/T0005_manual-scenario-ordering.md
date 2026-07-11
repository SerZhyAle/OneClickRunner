# Strategic spec: T0005 - Manual scenario ordering

**Ticket:** T0005
**Proposal:** A3
**Status:** Draft
**Priority:** 65
**Date:** 2026-07-11
**Tier:** Moderate
**Tactical plan:** `PLAN/T0005_manual-scenario-ordering/` (created by /spec-tech)

## 1. Problem
Scenarios appear - in the settings list and in the Jump List - in whatever order the OS returns
the per-scenario XML files from disk. The user cannot control that order, so the most-used
scenario cannot be pinned to the top of the Jump List.

## 2. Goals
1. The user can set an explicit order for scenarios (move up/down, or drag).
2. That order is what the settings list and the Jump List both use.
3. The order persists across restarts.

**Non-goals:**
- Grouping/folders or categories.
- Sorting modes (alphabetical, most-recent) - a possible later ticket.

## 4. Current architecture context
`ConfigurationService.LoadConfiguration` enumerates `*.xml` via `Directory.GetFiles` (disk
order) and returns the list as-is. `AppItem` has no order field. `App.BuildJumpList` and
`MainWindow` render in that list order.

## 5. Proposed approach
Give each scenario a persisted ordinal, keep the in-memory list sorted by it, and add list
controls to change a scenario's position. Persist the new ordinal on reorder. The Jump List
build reads the same ordered list, so both surfaces agree.

### 5.1 Pillars
- Persistence: an order value on the scenario model + save on change.
- UI: reorder affordance in the settings list (buttons and/or drag).
- Consumers: settings list and Jump List both honour the order.

## 6. Open questions
1. **Reorder affordance** - up/down buttons (simple) vs drag-and-drop (nicer, more work) vs both. To decide at approval.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Existing scenarios have no ordinal | High | Undefined initial order | Seed ordinals from current load order on first run |
| Import (T0008 counterpart) assigns no ordinal | Medium | New item lands unordered | Append imported items at the end |

## 9. Architecture decisions
**ADR-1: where order lives** - Decision: a field on the persisted scenario. Alternatives: a
separate order index file. Why: keeps the one-file-per-scenario model self-contained and
Import-friendly.

## 10. Links to other specs
- Interacts with Import/Export: T0008.
- Shares the list surface with: T0011, T0016, T0022.

## 11. Done criteria (strategic)
1. Moving a scenario up in the settings list moves it up in the Jump List after refresh.
2. The chosen order survives an app restart.

## 12. Next step
`/spec-tech T0005`.
