# Strategic spec: T0022 - Search / filter box in the list

**Ticket:** T0022
**Proposal:** A8
**Status:** Implemented
**Priority:** 35
**Date:** 2026-07-11
**Tier:** Easy
**Tactical plan:** `PLAN/T0022_list-search-filter/` (created by /spec-tech)

## 1. Problem
The settings list has no search or filter. With many scenarios, finding one means scanning the
whole list.

## 2. Goals
1. A search box filters the list live by name (and optionally path) as the user types.
2. Clearing the box restores the full list.

**Non-goals:**
- Filtering the Jump List itself (that is Windows-owned).
- Advanced query syntax.

## 4. Current architecture context
`AppListView.ItemsSource` is set to the full `AppItem` list in `MainWindow.LoadAppItems`. A
filter can sit over that collection view without changing storage.

## 5. Proposed approach
Add a search TextBox above the list and apply a case-insensitive filter over the list's
collection view on text change. Storage and the Jump List are untouched.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Filter state interacts oddly with reorder (T0005) | Low | Confusing moves while filtered | Disable reorder while a filter is active, or reorder underlying list |

## 10. Links to other specs
- Shares the list surface with T0005, T0011, T0016.

## 11. Done criteria (strategic)
1. Typing in the search box narrows the list to matching scenarios in real time.
2. Emptying the box shows all scenarios again.

## 12. Next step
Done.

**Result (2026-07-11):** Implemented. A **Search** box in the header filters the list's default
`ICollectionView` live (`FilterScenario`, case-insensitive over Name + Path); emptying it restores the
full list. The filter is re-applied in `LoadAppItems` because `ItemsSource` is replaced on every reload.
Storage and the Jump List are untouched. Reorder (T0005) still operates on the underlying full list, so
moving a scenario while filtered is safe (spec's interaction risk). Release build: 0 errors.
