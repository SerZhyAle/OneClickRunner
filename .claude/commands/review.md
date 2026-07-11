# Code Review

> **GLOBAL DIRECTIVES (anti-bureaucracy):**
> 1. Findings first, ordered by severity. No praise padding, no throat-clearing.
> 2. Every finding is actionable: a concrete fix or a precise question.
> 3. Default scope is the current diff / recently changed files, not the whole repo.

Review changed code for correctness and quality. For terse one-line-per-finding output,
use `/caveman-review`; this skill is the fuller pass.

## Usage

```text
/review [optional: file, diff, PR, or topic]
```

- `/review` - review the current diff
- `/review src/payments/` - scope to a path
- `/review correctness only` - restrict the dimensions

## What to check (in order)

1. **Correctness** - logic bugs, off-by-one, wrong conditionals, races, null/None handling,
   error paths, resource leaks, broken invariants.
2. **Security & data safety** - injection, unvalidated input, secrets in code, unsafe
   deserialization, destructive ops without guards, migration safety.
3. **Architecture** - dependency direction respected (`Windows/App (UI) -> Services -> Models`); entry points stay
   thin; logic lives in the right layer; no cross-layer leakage.
4. **Tests** - does the change have coverage for the new path and its failure modes? Flag
   missing/weak assertions.
5. **Anti-slop** - the seven greppable patterns in `doc/CODE_QUALITY.md` (the canonical list).
6. **Clarity** - naming, dead code, comment quality (why not what), file-size budget.

## Process

1. Identify the changed surface. Read the diff and enough surrounding code to judge it -
   do not review lines in isolation.
2. Verify claims against the code; never invent a line number or symbol.
3. For each finding emit: `severity` · `file:line` · the problem · a concrete fix.
   Severities: `bug` > `risk` > `arch` > `test` > `nit` > `q`.
4. Confirm or refute your own high-severity findings before reporting them - a plausible
   bug that does not actually reproduce is noise.
5. If you find nothing material, say so and name the residual risks (untested path,
   uncovered edge case) rather than padding.

## Output

- Findings grouped by severity, highest first.
- A short "what's solid" line only if it carries information (e.g. "migration is reversible
  and guarded").
- One closing line: the single most important thing to address before merge.
