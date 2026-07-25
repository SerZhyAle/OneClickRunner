# Spec: T0010 - Update README to match implementation

**Ticket:** T0010 · **Proposal:** D5 · **Status:** Implemented · **Priority:** 50 · **Tier:** Quick Win · **Date:** 2026-07-11
**Complexity:** Primitive (docs only, one file) - implement directly on approval, no tactical plan.

## Problem
`README.md` describes a system tray and a single `%APPDATA%\OneClickRunner\config.json`. The
implementation actually uses Windows **Jump Lists** as the launcher UI and stores **one XML file
per scenario** under `%APPDATA%\OneClickRunner\Scenarios\`. The README misleads any reader
(including future agents) about the architecture. CLAUDE.md already records this as a caveat.

## Approach
- `README.md` - replace the tray description with the Jump List model (right-click the taskbar
  icon; Settings/Exit tasks), correct the storage section to per-scenario XML under
  `Scenarios\`, and align the "How to Use" steps with the actual flow.
- Keep it truthful to **today's** behaviour. If T0009 (tray) is later approved, the tray parts
  return then - do not pre-document unbuilt features.

## Done criteria
- README no longer mentions a system tray or `config.json` as current behaviour.
- README correctly states Jump List launching and per-scenario XML storage.

## Links
- Overlaps with T0009 (tray): if that ships, revisit this doc.

**Result (2026-07-11):** Implemented on branch `chore/import-agent-kit`. Release build: 0 errors.
