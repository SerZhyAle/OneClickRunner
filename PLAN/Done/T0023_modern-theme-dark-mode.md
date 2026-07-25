# Strategic spec: T0023 - Modern theme / dark mode

**Ticket:** T0023
**Proposal:** A9
**Status:** Implemented
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
Done.

**Result (2026-07-11):** Implemented as a minimal in-repo `ResourceDictionary` (ADR-1: no external
dependency). `Themes/LightColors.xaml` + `Themes/DarkColors.xaml` share brush keys; `Themes/Styles.xaml`
holds app-wide implicit styles (Window, Button, TextBox, ComboBox, CheckBox, GroupBox, ListView,
GridViewColumnHeader, TextBlock) referencing those keys via `DynamicResource`. `Services/ThemeService`
picks the palette from the Windows `AppsUseLightTheme` setting and swaps it live on
`SystemEvents.UserPreferenceChanged` (**scope decision: follow the OS automatically**, no manual toggle).
Verified at runtime: a fresh launch logged `Theme applied: dark` on a dark-themed OS. Release build:
0 errors. Note: a styling layer over default control templates - popups (context menu) keep their
default chrome.

**Review follow-up (2026-07-11):** Runtime verification (window screenshots on a dark-themed OS) plus
the adversarial review exposed two real theming defects, both fixed and re-verified visually:
1. **Window backgrounds stayed white.** An implicit `Style TargetType="Window"` does not reach `Window`
   *subclasses* (`MainWindow`, `AppItemDialog`, `LinkInputDialog`), so their backgrounds were default
   white while the light label text became unreadable. Fixed by setting
   `Background`/`Foreground="{DynamicResource ...}"` on each window element directly (the dead implicit
   Window style was removed); hardcoded `Foreground="Gray"` labels now use `SubtleForegroundBrush`.
2. **ComboBox unreadable in dark mode** (review, low): a non-editable ComboBox keeps the hardcoded light
   Aero chrome regardless of `Background`. Fixed with a minimal themed `ControlTemplate` (toggle +
   content presenter + popup) plus a `ComboBoxItem` style. Verified: the "Scenario type" combo now
   renders dark with readable text.
