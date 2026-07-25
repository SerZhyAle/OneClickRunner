# Strategic spec: T0001 - Unify scenario launching; Jump List respects RunAsAdmin

**Ticket:** T0001
**Proposal:** C1 + D1 (merged - the shared launcher is the fix for the elevation bug)
**Status:** Implemented
**Priority:** 90
**Date:** 2026-07-11
**Tier:** Moderate
**Tactical plan:** `PLAN/T0001_unify-launcher-elevation/` (created by /spec-tech)

> Scope: STRATEGIC. What/why only.

## 1. Problem
The same scenario launches with different privileges depending on how it is started. Launching
from the Jump List / command line always requests elevation (`Verb = "runas"`), so every task -
even `calc.exe` - triggers a UAC prompt. Launching from the in-window **Run** button only
elevates when the scenario's `RunAsAdmin` flag is set. The user cannot predict whether a
scenario will prompt for admin, and non-admin scenarios are needlessly over-privileged from the
Jump List.

## 2. Goals
1. A scenario elevates **iff** its `RunAsAdmin` flag is set, regardless of launch path (Jump
   List, command line, or Run button).
2. Both launch paths share a single code path, so their behaviour cannot drift again.
3. No UAC prompt for a non-admin scenario started from the Jump List.

**Non-goals:**
- Changing what `RunAsAdmin` means or adding per-launch elevation overrides.
- Reworking the yt-dlp special case (see T0003 / T0013) beyond routing it through the shared launcher.

## 4. Current architecture context
Launch logic is duplicated: `App.HandleCommandLineArgs` (App.xaml.cs) builds a
`ProcessStartInfo` with a hardcoded `Verb = "runas"`, while `MainWindow.RunButton_Click`
(MainWindow.xaml.cs) builds a near-identical one that sets `Verb = "runas"` only when
`selectedItem.RunAsAdmin`. Two copies, one of which ignores the flag - that is the defect.

## 5. Proposed approach
Introduce one launch helper in the Services layer that takes a scenario plus an explicit
elevation decision derived solely from `RunAsAdmin`, and have both entry points call it.
Working directory, argument passing, and error handling live in that single place. The Jump
List path drops its unconditional elevation and passes the flag through.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| A scenario that relied on always-elevated Jump List launches stops working | Medium | User must set RunAsAdmin on it | Note in changelog; the flag is discoverable via T0011 |
| Behavioural change surprises the user | Low | Confusion | Document in README (T0010) |

## 10. Links to other specs
- Enables consistent display of the flag: T0011.
- Shares the launcher with: T0003 (yt-dlp), T0004 (path validation), T0006 (failure notice), T0014 (App split).

## 11. Done criteria (strategic)
1. Starting a `RunAsAdmin = false` scenario from the Jump List launches it with **no** UAC prompt.
2. Starting a `RunAsAdmin = true` scenario from either path shows the UAC prompt and runs elevated.
3. The Run button and the Jump List produce identical process start behaviour for the same scenario.

## 12. Next step
Done. Consider tactical follow-ups T0002/T0006/T0014, which build on the same seam.

**Result (2026-07-11):** Implemented on branch `chore/import-agent-kit`. New `Services/ScenarioLauncher.cs`
is the single launch path; elevation is set (`Verb = "runas"`) **iff** `AppItem.RunAsAdmin`. Both
`App.HandleCommandLineArgs` (Jump List / CLI) and `MainWindow.LaunchItem` (Run button, double-click)
call it, so the Jump List no longer unconditionally elevates. The duplicated `ProcessStartInfo`
blocks were removed. Release build: 0 errors.
