# Strategic spec: T0014 - Split App.xaml.cs into services

**Ticket:** T0014
**Proposal:** D2
**Status:** Draft
**Priority:** 45
**Date:** 2026-07-11
**Tier:** Moderate
**Tactical plan:** `PLAN/T0014_split-app-services/` (created by /spec-tech)

## 1. Problem
`App.xaml.cs` (~415 lines) mixes four concerns: startup/single-instance orchestration, named-pipe
IPC, Jump List construction, and scenario process launching. It is the largest file in the
project and violates the "thin entry points, delegate to services" rule in CLAUDE.md. The tangle
is also why the launch logic drifted (T0001).

## 2. Goals
1. `App` stays a thin orchestrator; each concern moves to a named Service class.
2. No behavioural change - pure structural refactor.
3. The extracted seams make T0001/T0002/T0006 cleaner to implement.

**Non-goals:**
- Changing behaviour, protocol, or UI.
- Introducing a DI container (manual composition is fine at this size).

## 4. Current architecture context
`App.xaml.cs` owns `OnStartup`, `HandleCommandLineArgs`, `StartPipeListener` (+ dead
`StartPipeServer`, see T0019), `BuildJumpList`/`RefreshJumpList`, and `OnExit`. `Services/`
already hosts `ConfigurationService`, `AutostartService`, `LoggingService` - the natural home
for the new pieces.

## 5. Proposed approach
Extract cohesive services: a Jump List builder, a pipe IPC listener, and a scenario launcher
(the T0001 launcher is the same seam). `App` wires them together and routes commands. Keep each
new class focused and under the size budget.

### 5.1 Pillars
- JumpListService (build/refresh).
- PipeIpcService (listen, dispatch to a handler).
- ScenarioLauncher (shared launch path - coordinates with T0001).

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Refactor changes behaviour subtly | Medium | Regressions in startup/IPC | Do it as ordered phases with a smoke run after each |
| Overlap with T0001/T0002 if done separately | High | Rework | Sequence: land T0001/T0002 first, or fold them into this refactor |

## 10. Links to other specs
- Strongly coupled with T0001 (launcher) and T0002 (pipe); removes code that T0019 deletes.

## 11. Done criteria (strategic)
1. `App.xaml.cs` no longer contains the Jump List, pipe, and launch bodies - they live in services.
2. Startup, Jump List launching, `/settings`, and `/exit` all still work (smoke-verified).
3. No file introduced exceeds the ~500-line budget.

## 12. Next step
`/spec-tech T0014`.
