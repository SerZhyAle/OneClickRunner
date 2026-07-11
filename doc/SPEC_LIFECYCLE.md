# The Spec Lifecycle - tooling-agnostic

This is the methodology behind the `/spec*` skills. It needs no special tooling: a ticket is
one Markdown file, and a status is a line in that file you keep honest. If you later want a
JSONL index, a CLI, or a database behind it, fine - the method does not depend on it.

## Why split strategy from tactics

A spec answers two different questions that get muddled when written together:

- **Strategic (what / why):** the problem, the goals, the constraints, the open questions.
  No class names, no file paths, no signatures. If a reviewer disagrees here, you saved
  yourself an implementation.
- **Tactical (how):** the exact files, symbols, and the *order* to build them, each step
  ending in a check anyone can run. If a step is ambiguous here, the executor stops instead
  of guessing.

Keeping them in separate documents is the single highest-leverage habit in this kit. The
strategic doc is the contract; the tactical doc is the build instructions; the audit checks
the building against the contract.

## The status flow

```
Draft ──► Approved ──► Tactical ──► In Progress ──► Implemented ──► Verified
   │                                                      │
   └─ rough is fine here                                  └─ audit may instead yield:
                                                             Partial  (warnings, no failures)
                                                             Broken   (at least one failure)
```

Plus explicit block states, each entered with a one-line note:

- `BlockNeedUserTest` - built, awaiting a hands-on / manual check.
- `BlockByOtherTask` - depends on another ticket.
- `BlockQuestions` - awaiting a decision from the owner.
- `BlockExternal` - waiting on a library release, hardware, or a third party.

Block states need a defined exit. To clear a `BlockQuestions` ticket: isolate the decisions
blocking the next transition, silently settle any the codebase or prior research already
determines, put only the genuine judgment calls to the owner (offer a researched default first),
record the answers back into the ticket, and advance exactly one level - then stop. Never invent
an owner's answer. The other block states clear the same way: remove the condition, note it, and
advance one level.

Add a reversible terminal status, `Archived`: move a cancelled or superseded ticket's files out
of the active workspace but keep its record, so work stays findable and restorable. Never
hard-delete a record. Archiving is also the last sweep for verification tags - a ticket can be
archived from a non-verified state, so clear any leftover tag (see "Verification tags" below) as
part of archiving, or the "tag exists iff awaiting test" invariant breaks on the cancel path.

The status is the **first** `**Status:**` line in the file. It is true because the code makes
it true, never because the filename or a wish says so.

## The complexity ladder

The gate is not binary - it is a ladder of rungs, and the rule is **pick the lowest rung that
still proves the change**: a typo is `/quick`; a known bug is `/fix`; a small, deterministic
change is a three-section PRIMITIVE spec; anything with real unknowns is the full COMPLEX
pipeline. `/quick` and `/fix` need no spec at all.

To choose between the top two rungs, score the task:

- ≤ 3 existing files change, no new files
- no new public types
- no schema/migration change
- no new dependency-injection wiring
- no new screen/route/destination
- mechanically deterministic - no deferred decisions
- under ~100 lines of delta

**All true → PRIMITIVE:** write a three-section spec (Problem / Approach / Done criteria) and
implement directly. **Any false → COMPLEX:** run the full pipeline. The ladder is what keeps
the ceremony proportional to the work.

## Designing the phase graph (the part that goes wrong)

Phase ordering is where tactical plans fail. Three passes prevent it:

1. **Coverage inventory.** List every goal, pillar, constraint-with-impact, resolved research
   finding, ADR, and criterion. Map each to a phase, or mark it `out-of-scope: <reason>`. An
   unmapped line means the plan is incomplete.
2. **Produces / Consumes topology.** For each phase, list what it produces (new/changed
   artifacts) and what it consumes. No phase may consume what a *later* phase produces. A
   forward reference means the order is wrong - reorder before writing anything.
3. **Real-work filter.** Every step's primary action changes source/resources/config. Steps
   that only edit plan text, "review docs", or restate a previous step are not steps. The one
   exception is the final docs-cleanup phase.

Then a mandatory **self-review**: re-read the written plan against the inventory and the
topology. Every inventory line maps to a *written* step; every consumed symbol greps in the
code or is produced earlier; every dependency matches the topology. Fix in place. This pass
catches the phase-order bug that would otherwise cost a full execution cycle.

## Verifiable steps

Every step ends in a **static** check - file exists, symbol declared, value equals, command
exits 0 - never "works correctly". A step is done only when its check passes *in this run*,
not when you intended it. The executor runs one step, runs its check, and hard-stops on the
first failure or ambiguity rather than guessing forward.

## Verification tags (optional but neat)

When a ticket needs manual testing before it can be called done, insert a temporary log line
at each changed-flow entry point:

```
LoggingService.Log("<ID>: <what this code path proves>")
```

The invariant: such a tag exists **iff** the ticket is currently `BlockNeedUserTest`. You
insert them as the last code edits before the final build (so one build validates code +
tags), and the audit deletes them when the ticket leaves that status. Permanent logs never
embed a ticket id. The payoff: during a manual run you grep the log for `<ID>:` and *see*
which paths actually executed. It complements automated tests, it does not replace them - reach
for it only where a test cannot observe which path a manual or on-device run took. Keep each tag a
single greppable line so removal stays mechanical and safe.

## The skills, end to end

```
/research      gather evidence, persist findings           (before anything non-trivial)
/spec          write the strategic what/why                Draft -> Approved
/spec-tech     turn it into a phased, verifiable plan       Approved -> Tactical
/spec-dev      execute one step at a time, check each       Tactical -> Implemented / BlockNeedUserTest
/spec-check    audit the build against the spec             -> Verified / Partial / Broken
/spec-fix      apply the audit's action items, re-audit     Partial / Broken -> (re-check)
/verify        run it and observe, when behaviour matters   (read-only on status)
```

`/quick` and `/fix` sit *below* this pipeline for changes too small to deserve it. `/ui-clarify`
sits *before* it whenever a user-facing decision is unresolved.

## Status gates (the one rule)

Every skill reads and writes the same ticket status, so the gates live here once. Each skill
**requires** an entry status, **produces** an exit status, and **auto-chains** to the next skill
unless a stop condition fires. This table is the single source of truth - a skill restates only
its own row, and `VALIDATION.md` and the skill files defer to it rather than redefining the flow.

| Skill | Requires | Produces | Auto-chains to | Stops instead when |
| --- | --- | --- | --- | --- |
| `/research` | any | no status change | the caller | - |
| `/spec` | none (allocates an id) | `Approved` (complex), or `Implemented` / `BlockNeedUserTest` (primitive, implemented in place) | `/spec-tech` (complex only; a primitive stops after implementing) | a required research item is Open |
| `/spec-tech` | `Approved` or later | `Tactical` | `/spec-dev` | an unchecked pre-implementation blocker remains |
| `/spec-dev` | `Tactical` / `In Progress` | `Implemented` or `BlockNeedUserTest` | `/spec-check` | ambiguity, a failed check, or a `Block*` condition |
| `/spec-check` | `Implemented` or later | `Verified` / `Partial` / `Broken` / `BlockNeedUserTest` | `/spec-fix` (only if `Partial`/`Broken`) | an open manual check holds at `BlockNeedUserTest` |
| `/spec-fix` | `Partial` / `Broken` | re-runs `/spec-check` | `/spec-check` | a fix needs a decision the audit cannot supply |
| `/verify` | any | no status change | - | - |

Three rules bind the whole table:

- **Auto-chain by default; stop only at a real decision.** The pipeline flows on its own and
  pauses only at the encoded conditions above - an open required question, an unchecked blocker,
  an ambiguity. It does **not** wait for a sign-off at every stage; that would be bureaucracy, not
  safety. `/spec` records its own `Approved` flip and continues. To review between stages, run the
  skills individually or pass `--dry-run`.
- **`Verified` means nothing is left open.** A ticket reaches `Verified` only when every check is
  PASS or EXEMPT - zero FAIL, zero WARN, and **no open MANUAL item**. An unresolved manual /
  on-target signal keeps the ticket at `BlockNeedUserTest` (or `Partial`) until a human closes it
  and a re-audit turns it green. Status comes from reality, so an open manual check can never sit
  beneath a `Verified`.
- **Entry precondition for the verification states.** A ticket may enter `BlockNeedUserTest` or
  `Implemented` only when its headline user-visible behaviour already works end-to-end. Created
  classes, wired contracts, and a passing compile are milestones, not deliverables - never invite a
  human to test while the advertised action still only logs, shows a placeholder, or no-ops. That
  wastes their time and corrupts the meaning of the verification states.
- **Two branches leave the straight line, and the rows above encode them.** A *primitive* `/spec`
  (the complexity gate) implements without a tactical plan and stops at `Implemented` or
  `BlockNeedUserTest` - it never enters `/spec-tech`. A sound build with an unresolved manual check
  produces `BlockNeedUserTest`, not `Verified`. Everything else follows the linear flow.

## Draining BlockNeedUserTest

The lifecycle accumulates human-gated tickets by design: every ticket whose headline behaviour
needs a real run parks at `BlockNeedUserTest` until a human closes it. Left alone they pile up, so
sweep them in a batch rather than one interruption at a time:

1. Group every ticket currently at `BlockNeedUserTest`.
2. For each, run the recorded manual check - its verification tags (grep `<ID>:`) say exactly which
   paths to exercise on the real run.
3. Re-audit the batch with `/spec-check`: the ones whose manual signal is now closed flip to
   `Verified` and their tags are removed in the same pass; the rest stay blocked with a note.

Keep it a periodic sweep, not a per-ticket prompt - deferring the human gate to one batch is the
whole reason the block state exists. A lean `/sweep` command (or a filter over `PLAN/`) is
enough; no new status model is needed.

## Adapting it

- Replace `<ID>` with whatever id scheme you like (`T0042`, `JIRA-123`, a date-slug).
- Replace `PLAN` with your docs location.
- If you do not want block states, drop them - the linear flow still works. `Archived` is
  optional in the same way; without it, just stop touching a dead ticket and leave its record.
- If your team already has tickets in an external tracker, keep the *file* as the spec and
  link the tracker id; the discipline is in the document, not the tool.
- If you *do* build tooling around this (a ledger, a CLI, a status command), make the
  consequential transitions gates that **refuse** when their preconditions are missing - a
  promotion that cannot fire while a required question is open, a block that cannot be entered
  without a recorded reason - rather than labels a reviewer must remember. The discipline works
  by hand; tooling just makes it harder to skip.
