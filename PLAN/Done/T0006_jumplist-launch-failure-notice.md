# Strategic spec: T0006 - Failure notification for Jump List launches

**Ticket:** T0006
**Proposal:** B2
**Status:** Implemented
**Priority:** 60
**Date:** 2026-07-11
**Tier:** Easy
**Tactical plan:** `PLAN/T0006_jumplist-launch-failure-notice/` (created by /spec-tech)

## 1. Problem
A Jump List launch runs in a short-lived one-shot process with no window. When the launch fails
(missing path, denied elevation, bad arguments) the only trace is a line in `activity.log`; the
user gets no feedback and assumes nothing happened.

## 2. Goals
1. A failed Jump List launch produces a visible, transient notification naming the scenario and reason.
2. A successful launch stays silent (no noise).

**Non-goals:**
- A full notification/history center.
- Changing how the in-window Run button reports errors (it already shows a MessageBox).

## 4. Current architecture context
`App.HandleCommandLineArgs` catches launch exceptions and only calls `LoggingService.Log`. The
one-shot process may exit before anything is shown. Note the elevation/validation changes
(T0001, T0004) determine what counts as a failure here.

## 5. Proposed approach
On a launch failure in the command/Jump List path, surface a lightweight OS notification (toast)
or an equivalent transient message before the process exits, carrying the scenario name and a
short reason. Keep success paths silent.

## 6. Open questions
1. **Mechanism** - Windows toast (needs a bit of plumbing on WPF) vs a simple top-most transient
   window vs balloon. Decide at approval.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| One-shot process exits before the toast is delivered | Medium | No message shown | Keep the process alive until the notice is dispatched |

## 10. Links to other specs
- Consumes failure signals from T0004; pairs with T0001.
- If T0009 (tray) lands, the tray icon is the natural notification source.

## 11. Done criteria (strategic)
1. Launching a Jump List task whose path is missing shows a visible notice naming the scenario.
2. Launching a valid task shows no notice.

## 12. Next step
Done.

**Result (2026-07-11):** Implemented as `Windows/NotificationWindow` - a small, topmost,
self-dismissing toast (7 s, click to dismiss, bottom-right of the work area). `App.HandleCommandLineArgs`
shows it when `ScenarioLauncher.Launch` returns a non-cancelled failure, carrying the scenario name and
reason. It is shown **modally** (`ShowDialog`) so the windowless one-shot process stays alive until the
notice has been delivered (the spec's key risk). Successful launches and user-cancelled ones stay
silent. **Mechanism decision:** a lightweight in-app transient window rather than a Windows toast, so it
works without WinRT/AppUserModelID plumbing and delivers reliably from a one-shot process. Release
build: 0 errors.

**Review follow-up (2026-07-11):** The review noted the modal `ShowDialog` also fired from the live
tray/pipe launch path, freezing the settings window for up to 7 s. `ShowTransient` now takes
`blockUntilClosed`: modal only in the windowless one-shot process (`_mainWindow == null`), non-modal in
a live instance so it never blocks the UI.
