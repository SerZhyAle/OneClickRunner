# Strategic spec: T0021 - Jump List icons for script scenarios

**Ticket:** T0021
**Proposal:** B1
**Status:** Draft
**Priority:** 35
**Date:** 2026-07-11
**Tier:** Easy
**Tactical plan:** `PLAN/T0021_jumplist-script-icons/` (created by /spec-tech)

## 1. Problem
Each Jump List task sets `IconResourcePath = item.Path`. That yields an icon only when the path
is an executable carrying one. For script scenarios (`.ps1`, `.bat`, `.cmd`, `.py`) there is no
embedded icon, so those tasks appear blank, making the Jump List look inconsistent.

## 2. Goals
1. Script scenarios get a sensible icon in the Jump List (e.g. the interpreter's icon or a bundled fallback).
2. Executable scenarios keep their own icon.

**Non-goals:**
- Per-scenario custom icon picking (possible later ticket).

## 4. Current architecture context
`App.BuildJumpList` sets `IconResourcePath`/`IconResourceIndex` from `item.Path`. Icon choice is
therefore a function of the scenario's file type, computed at Jump List build time.

## 5. Proposed approach
Choose the icon by scenario file type: use the target's own icon when it is an executable,
otherwise fall back to the associated interpreter's icon or a small bundled default. Compute this
where the Jump List (and, if built, the tray menu T0009) is assembled.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Interpreter path not resolvable | Low | Blank again | Fall back to a bundled default icon |

## 10. Links to other specs
- The app currently ships no icon asset (see also T0009); a bundled fallback may be shared.

## 11. Done criteria (strategic)
1. A `.ps1`/`.bat` scenario shows a non-blank icon in the Jump List.
2. An `.exe` scenario still shows its own icon.

## 12. Next step
`/spec-tech T0021`.
