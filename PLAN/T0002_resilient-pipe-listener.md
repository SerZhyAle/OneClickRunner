# Strategic spec: T0002 - Resilient named-pipe listener

**Ticket:** T0002
**Proposal:** C2
**Status:** Draft
**Priority:** 85
**Date:** 2026-07-11
**Tier:** Easy
**Tactical plan:** `PLAN/T0002_resilient-pipe-listener/` (created by /spec-tech)

## 1. Problem
The running instance listens for commands from secondary launches on a named pipe. The
listener loop's `try/catch` wraps the **entire** `while (true)`, so a single failed connection
or read throws out of the loop and the listener dies permanently. After that, Jump List
`/settings` and `/run` commands sent to the already-running instance are silently dropped until
the app is restarted.

## 2. Goals
1. A failure handling one pipe connection does not stop the listener from accepting the next.
2. The listener keeps serving commands for the whole app lifetime.
3. Per-connection failures are logged, not fatal.

**Non-goals:**
- Changing the command protocol or the single-instance/mutex design.
- Removing the unused `StartPipeServer` (that is T0019).

## 4. Current architecture context
`App.StartPipeListener` (App.xaml.cs) runs a background task with `while (true) { create
server; WaitForConnection; read; dispatch }`, all inside one outer `try/catch` that logs and
exits on any exception. There is no per-iteration isolation.

## 5. Proposed approach
Move the failure boundary inside the loop so each accept/read/dispatch cycle catches and logs
its own exception and then continues to the next connection. Keep an outer guard only for an
unrecoverable condition. Ensure the loop can be ended cleanly on app shutdown rather than by an
unhandled exception.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| A tight failure loop spins if the pipe is permanently broken | Low | CPU burn | Log-and-continue with the OS blocking on WaitForConnection; add a guard if a rapid repeat is detected |

## 10. Links to other specs
- Related dead-code cleanup: T0019.

## 11. Done criteria (strategic)
1. Forcing an exception on one pipe connection leaves the listener able to serve the next command.
2. After a handled pipe error, sending `/settings` to the running instance still restores the window.

## 12. Next step
`/spec-tech T0002`.
