# Strategic spec: T0006 - Failure notification for Jump List launches

**Ticket:** T0006
**Proposal:** B2
**Status:** Draft
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
`/spec-tech T0006`.
