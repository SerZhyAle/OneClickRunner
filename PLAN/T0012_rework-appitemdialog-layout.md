# Strategic spec: T0012 - Rework AppItemDialog layout; make resizable

**Ticket:** T0012
**Proposal:** A6
**Status:** Implemented
**Priority:** 50
**Date:** 2026-07-11
**Tier:** Moderate
**Tactical plan:** `PLAN/T0012_rework-appitemdialog-layout/` (created by /spec-tech)

## 1. Problem
The Add/Edit dialog places each label and its input field in the **same** grid row and separates
them only with hardcoded `Margin` offsets (e.g. label at top, textbox pushed down 20-30px within
one `Auto` row). This overlaps-by-margin layout is fragile: a font-size or DPI change misaligns
labels and fields. The window is also fixed-size (`ResizeMode="NoResize"`, 350x500), so long
paths are truncated with no way to widen.

## 2. Goals
1. Each label sits in its own row above its field, via real grid rows - no margin-offset hacks.
2. The dialog is resizable, and fields grow with width so long paths are readable.
3. No behavioural/field change - same inputs, same validation.

**Non-goals:**
- Adding new fields (scenario type, order) - those are other tickets.
- Restyling/theming (T0023).

## 4. Current architecture context
`OneClickRunner/Windows/AppItemDialog.xaml` - a 6-row grid where labels and textboxes share
rows and are positioned by `Margin`. `AppItemDialog.xaml.cs` reads/writes the same named
controls; the rework is XAML-side and must preserve those control names/bindings.

## 5. Proposed approach
Restructure the grid so every label/field pair uses dedicated rows (or a two-column
label/field layout), remove the margin offsets, allow resize, and let the path/working-dir
fields stretch. Preserve control names so the code-behind is untouched (or minimally touched).

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Renaming controls breaks code-behind bindings | Medium | Compile/runtime break | Keep existing `x:Name`s |
| Resize reveals stretch bugs | Low | Cosmetic | Test min/expanded sizes |

## 10. Links to other specs
- If T0013 adds a scenario-type selector, it lands in this reworked layout.
- Visual restyle is separate: T0023.

## 11. Done criteria (strategic)
1. Labels and fields stay aligned at 100% and 150% display scaling.
2. The dialog can be resized wider and the path field grows to show a long path.
3. Add and Edit still save the same fields as before.

## 12. Next step
Done; the yt-dlp type selector (T0013) landed in this reworked layout.

**Result (2026-07-11):** Implemented. `AppItemDialog.xaml` now stacks each label on its own line above
its field (no margin-offset overlap); fields live in `*`-width grid columns so they stretch. The window
is `ResizeMode="CanResize"` with `MinHeight`/`MinWidth`, so it widens and long paths become readable.
All `x:Name`s were preserved, so the code-behind's control access is unchanged. Release build: 0 errors.
