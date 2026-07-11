# Strategic spec: T0023 - Modern theme / dark mode

**Ticket:** T0023
**Proposal:** A9
**Status:** Draft
**Priority:** 30
**Date:** 2026-07-11
**Tier:** Strategic
**Tactical plan:** `PLAN/T0023_modern-theme-dark-mode/` (created by /spec-tech)

## 1. Problem
The UI uses default WPF control styling, which looks dated and does not follow the OS light/dark
preference. It is purely cosmetic but affects perceived quality.

## 2. Goals
1. A cohesive, modern visual style across the settings window and dialogs.
2. Support for a dark appearance, ideally following the Windows theme.

**Non-goals:**
- Functional/layout changes (layout is T0012).
- Per-control custom theming beyond a consistent style set.

## 4. Current architecture context
XAML across `MainWindow.xaml`, `Windows/AppItemDialog.xaml`, `Windows/LinkInputDialog.xaml` uses
stock styles and no shared resource dictionary. `App.xaml` has an empty resources section - the
natural place for app-wide styles/theme resources.

## 5. Proposed approach
Introduce shared theme resources (a resource dictionary, or a lightweight theming library) and
apply them app-wide, with light/dark variants keyed to the OS setting. Keep it a styling layer
over the existing controls.

## 6. Open questions
1. **Approach** - hand-rolled `ResourceDictionary` vs a WPF theming library (extra dependency).
2. **Scope** - follow OS theme automatically, or add a manual toggle.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| A theming dependency adds weight/licensing | Medium | Heavier app | Prefer a minimal resource dictionary unless a library clearly wins |
| Contrast/readability regressions in dark mode | Medium | Poor legibility | Check both themes on every window |

## 9. Architecture decisions
**ADR-1: library vs hand-rolled** - deferred to approval; default to a minimal in-repo
`ResourceDictionary` to avoid a new dependency unless a library is clearly justified.

## 10. Links to other specs
- Best sequenced after the layout rework T0012 so styling lands on a stable layout.

## 11. Done criteria (strategic)
1. All windows share one consistent, modern style.
2. A dark appearance is available and legible; if OS-driven, switching the Windows theme updates the app.

## 12. Next step
`/spec-tech T0023`.
