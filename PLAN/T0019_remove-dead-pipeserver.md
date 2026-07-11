# Spec: T0019 - Remove dead StartPipeServer method

**Ticket:** T0019 · **Proposal:** D3 · **Status:** Implemented · **Priority:** 40 · **Tier:** Quick Win · **Date:** 2026-07-11
**Complexity:** Primitive (1 file, deletion) - implement directly on approval, no tactical plan.

## Problem
`App.StartPipeServer` is never called - the active IPC path is `StartPipeListener`. The dead
method (and the `_pipeServer` field it uses) is duplicate machinery that misleads readers about
which pipe code is live. CLAUDE.md already flags it as dead.

## Approach
- `OneClickRunner/App.xaml.cs` - remove `StartPipeServer` and any field/using left orphaned by
  its deletion (e.g. `_pipeServer` if unused afterwards). Confirm no reference remains before
  deleting.

## Done criteria
- `StartPipeServer` no longer exists; the project builds.
- `grep` for `StartPipeServer` / orphaned `_pipeServer` returns zero hits.

## Links
- Do alongside or after T0002 / T0014, which touch the same IPC area.

**Result (2026-07-11):** Implemented on branch `chore/import-agent-kit`. Release build: 0 errors.
