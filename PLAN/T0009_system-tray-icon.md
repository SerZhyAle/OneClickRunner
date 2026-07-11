# Strategic spec: T0009 - System tray icon

**Ticket:** T0009
**Proposal:** B6
**Status:** Implemented
**Priority:** 55
**Date:** 2026-07-11
**Tier:** Strategic
**Tactical plan:** `PLAN/T0009_system-tray-icon/` (created by /spec-tech)

## 1. Problem
The app starts minimized and has no tray presence. Once minimized there is no obvious way back
to the settings window except a Jump List "Settings" task or relaunch. The README also promises
a tray that does not exist (see T0010). A tray icon gives a persistent home, quick access to
scenarios, and a place to restore/exit.

## 2. Goals
1. A tray icon is present while the app runs.
2. Left-click (or double-click) restores the settings window; right-click shows a menu.
3. The tray menu offers the scenarios plus Settings and Exit, mirroring the Jump List.

**Non-goals:**
- Removing the Jump List (the two can coexist).
- Rich tray UI beyond a context menu.

## 4. Current architecture context
WinForms is already enabled in the project (`UseWindowsForms`), so `NotifyIcon` is available
without new dependencies. Window lifecycle already minimizes-instead-of-closes
(`MainWindow.Window_Closing`), and scenario/command routing already exists
(`HandleCommandLineArgs`, `BuildJumpList`). A tray menu is a new consumer of the same
scenario list and command routing.

## 5. Proposed approach
Add a tray icon owned by the app lifetime, backed by the existing scenario list and command
routing. Its menu is rebuilt alongside the Jump List (same refresh trigger). Clicking a
scenario runs it through the shared launcher; Settings restores the window; Exit performs the
real shutdown (the confirmed-exit path).

### 5.1 Pillars
- Tray presence + lifecycle (create on start, dispose on exit).
- Menu built from the same ordered scenario list as the Jump List.
- Actions routed through existing command handling / the T0001 launcher.

## 6. Open questions
1. **Icon asset** - the app currently ships no custom icon; need one for the tray.
2. **Relationship to "start minimized"** - with a tray, should first run still show settings when empty? (Likely yes.)

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Icon leaks (stays after exit) if not disposed | Medium | Ghost tray icon | Dispose NotifyIcon in OnExit |
| Menu and Jump List drift | Medium | Inconsistent lists | Rebuild both from one refresh call |

## 9. Architecture decisions
**ADR-1: NotifyIcon (WinForms) vs a custom WPF tray** - Decision: WinForms `NotifyIcon`.
Why: WinForms is already enabled; no new dependency; least code.

## 10. Links to other specs
- Makes T0010 (README) accurate again; natural host for T0006 notifications.
- Menu shares the ordered list from T0005 and the launcher from T0001.

## 11. Done criteria (strategic)
1. While running, a tray icon is visible; double-click restores the settings window.
2. The tray menu lists the same scenarios as the Jump List and can run one.
3. Exiting the app removes the tray icon.

## 12. Next step
Done. Update README (T0010 counterpart) to mention the tray.

**Result (2026-07-11):** Implemented as `Services/TrayIconService` (WinForms `NotifyIcon`, per ADR-1),
owned by `App`. Double-click restores the settings window; right-click shows a menu listing the same
ordered scenarios as the Jump List, plus a separator, **Settings** (restore) and **Exit** (the
confirmed-exit path via the new `MainWindow.RequestExit`). The menu is rebuilt from the shared
`RefreshJumpList` call, so it cannot drift from the Jump List. The icon is disposed in `OnExit`
(no ghost). All actions route through caller-supplied callbacks, so the service keeps
`Services -> Models` layering. **Open questions:** icon = the existing `Assets/app.ico`; first run
still shows Settings when empty (unchanged). Release build: 0 errors.
