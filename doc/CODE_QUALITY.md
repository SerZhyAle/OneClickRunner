# Code Quality - anti-slop conventions

A short, enforceable list of the patterns an AI assistant (or a tired human) tends to emit
that look fine and quietly rot a codebase. The rule is not "review for these later" - it is
**do not write them in the first place**, and **flag them on sight** in review.

Adapt the concrete syntax to your language; the intent is universal.

## The seven slop patterns

1. **Trivial comments.** A comment that restates the adjacent line adds noise and goes stale.
   ```
   // increment i
   i++
   ```
   Comment *why*, not *what* - and only when the code cannot express it (a non-obvious
   business rule, a handled edge case, a workaround, an invariant). If the line is obvious,
   delete the comment.

2. **Empty or broad swallowing catches.** A catch that hides the error is a bug with a delay.
   ```
   try { risky() } catch (e) { /* ignore */ }
   ```
   Every catch must do one of: recover, return a documented safe default, or log a
   plain-language degradation at the right level. Catch the narrowest type you can. If you
   truly intend to ignore, say *why* in one comment - the reason, not the fact.

3. **Hardcoded values where a token exists.** A literal color, dimension, URL, or magic
   number inlined where the project has a theme attribute / constant / config entry. It
   breaks theming, environments, and reuse. Reference the token; if none exists and the value
   recurs, create one.

4. **Lifecycle-unsafe async / global mutable scope.** Launching work that outlives or
   races its owner - a coroutine/promise tied to nothing, a subscription never cancelled, a
   module-level mutable singleton where a scoped instance belongs. Use the project's scoped
   concurrency and lifecycle-aware collection. Never reach for a global scope because it is
   shorter.

5. **Non-facade logging.** `print`, `console.log`, `System.out`, `println` in shipped code.
   Route through the project's logging facade (`LoggingService.Log`) so level, format, and sink are
   controlled. Diagnostics you would be embarrassed to ship do not get committed.

6. **Shipped stubs.** `TODO()`, `throw NotImplementedError`, `fatalError("unimplemented")`,
   `raise NotImplementedError` on a path that actually runs. A stub on a live path is a crash
   waiting for a user. Either implement it or gate it so it cannot be reached, and track the
   gap in a ticket.

7. **Dead weight left behind.** A change that supersedes code/resources/config but leaves the
   old version, an orphaned key, or a build keep-rule naming a deleted symbol. Delete the
   remnant in the *same* change. Before deleting a zero-reference artifact, check it is not
   active scaffolding for an in-flight ticket. Verify "removed from the build" on the real
   target/release artifact, not a debug build.

## Structural rules

- **Dependency direction.** Respect `Windows/App (UI) -> Services -> Models`. An outer layer may depend on an inner
  one, never the reverse. No cross-layer shortcuts "just this once".
- **Thin entry points.** Controllers / activities / route handlers wire and delegate; they do
  not hold business logic. Logic lives in named helper/service classes.
- **File-size budget.** Past ~`500` lines, extract a cohesive helper. Size is a proxy
  for "this file does too many things".
- **Naming.** Match the codebase's existing convention exactly. Consistency beats your
  personal preference.
- **Scope of a change.** Fix the thing asked. No opportunistic refactor, no unrelated import
  cleanup, no drive-by reformatting - those drown the real diff and break `git blame`.

## Comment discipline (the one worth repeating)

A good comment explains a decision the code cannot: *why this order*, *why this guard*, *why
not the obvious approach*. A bad comment narrates the code, repeats the function name, or
describes what a reader can see. When you edit a region, read its existing comments first and
treat them as intent - do not silently invalidate them. Remove comments that have gone stale.

## Why this is a document, not a vibe

Each pattern above is greppable. A reviewer - human or agent - can scan a diff for empty
catches, inline hex, raw `print`, `TODO()`, and orphaned keys mechanically. Make the check
part of "done", not part of "someday". If your stack has a linter, encode as many of these as
rules so the gate is automatic and the assistant gets fast feedback.

## Adopting a rule on a codebase that already violates it

A zero-tolerance gate on legacy code either never merges or forces a risky mass cleanup. Gate
on NEW violations instead. Freeze the current violation count as a checked-in baseline; the
gate fails only when the count rises above it. The rule takes effect immediately - no blocking
cleanup - and the tooling may lower the baseline as you clean up but must refuse to raise it,
so debt ratchets downward only, never back up. This is the adoption mechanism for every rule
above: it is how you enforce a new invariant on legacy code without a big-bang cleanup.

**The ratchet can resurface accepted debt.** A baseline keyed by identity - file + line, or a
rule-plus-location hash - re-flags debt you already accepted when adjacent code shifts it: a
rename, a signature change, or a reformat moves the anchor, and the linter reports the same old
finding as new. Prefer a content- or count-based baseline where the tool allows it; and when a
resurfaced item is the same accepted debt, re-accept it rather than "fixing" churn just to satisfy
the gate. This trap is identical across detekt, ESLint, Ruff, and PHPStan.

## Close the source, not just the detector

A gate that catches a recurring defect is only half the loop: a detector stops a human once,
but an agent re-emits the pattern every generation. When a check keeps flagging the same thing,
add the matching DON'T to the always-loaded rulebook (`CLAUDE.md`) and reinforce it in the
code-generating skills - substance in the canonical rule, skills as short pointers - so the
agent stops producing the defect, not merely flagging it after the fact.

## Reference docs are a build artifact

User-facing reference docs are part of the change that alters behaviour, not a follow-up: the
same change updates the doc, and a check fails when behaviour and its documented description
drift apart. If your stack can generate the doc from the source of truth, gate on the
regenerated output; otherwise gate that the doc was touched whenever the matching behaviour
was. Keep mirrored docs in lockstep, and separate an always-append developer inventory of what
shipped from any curated outward-facing summary (regenerate the latter only at release).
