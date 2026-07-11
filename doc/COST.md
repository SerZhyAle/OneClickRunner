# Agent Cost & Fan-out - spend the budget where it pays

Agents burn two budgets: tokens and wall-clock. The method's autonomy multiplies both - a
research order that fans out, a pipeline that re-audits, a reviewer that spawns skeptics. This
document is the discipline that keeps the spend proportional to the value. It is not a cap; it is
a policy for deciding *when the spend is worth it*.

## Inline vs subagent - when to spawn

Do the work **inline by default**. A subagent is not free: it costs a full context spin-up plus
its own tokens, and its report is a claim you must re-validate. Spawn one only when it buys
something inline cannot:

- **Parallelism** over genuinely independent questions - local code and external docs at once.
- **Isolation** of bulky evidence - run a task with a large, noisy artifact trail in a throwaway
  agent that returns one compact verdict, so the raw material stays in the child instead of
  swelling your context.
- **A fresh context** the main thread should not have to carry.

For a quick lookup, an inline grep beats a spawned reader every time.

## Context hygiene

- The context window fills fast and answer quality degrades as it fills. Between unrelated tasks,
  start fresh (compact / clear) rather than dragging a stale context forward.
- **Offload raw artifacts** - full build logs, grep dumps, large file bodies - to `temp/`
  and reference them by path. Never paste them into the running context or a durable doc; keep the
  proof available, keep the context scannable.
- Journal at the grain of the logical change (see `VALIDATION.md`), not per file, so the narrative
  stays short.

## The fan-out budget gate

Before spawning a wave of parallel agents, gate it:

- **Estimate** the agent count and the rough token cost. A handful of readers is cheap; a dozen
  writers each dragging bulky evidence is not.
- **Keep a small default ceiling** (~6-8 agents per run). Above it, get an explicit GO - state the
  count, the cost, and why fewer will not do.
- **Stage the work: find first, then verify.** Do not fan out verification over findings you have
  not deduplicated - you will pay to check the same thing three times.
- **Never silently resume a run a limit killed.** A limit-killed fan-out leaves partial,
  unverified results. Report what completed and what did not, then decide - do not paper over the
  gap and call it done.

## Model-tier routing per skill

Route each skill to the cheapest model that still does its job well:

- **Mechanical leaf skills** - `/quick`, `/caveman-commit`, a status flip, a changelog line -
  run fine on a small, cheap model.
- **Diagnosis, review, orchestration, and spec design** - `/spec`, `/spec-check`, `/review`, the
  rd-lead orchestrator - stay on a strong model. A cheap model here costs more in wrong turns than
  it ever saves in tokens.
- The routing is a dial, not a law: when a "mechanical" skill meets real ambiguity, it escalates
  rather than guessing cheaply.

This is the operational half of the "Any model" idea - the method does not change with model
power, but *which* model you point at *which* skill does.

## Verification is not free either

Adversarial verification - several skeptics per finding - is the right spend on a claim that
would be expensive to get wrong, and waste on a trivial one. Match the depth to the cost of the
finding being false: a one-line typo fix earns a grep; a security or data-safety claim earns
independent skeptics who each address the named mechanism. Spending the same effort on both is how
a review gets slow, then gets skipped.

## Why this is a document, not a vibe

Cost discipline is easy to nod at and easy to blow past mid-task, when a fan-out "might help" and
the agents are one call away. Making it a written gate - estimate, ceiling, explicit GO - turns
"spawn a dozen agents" from a reflex into a decision a reviewer can point at. The goal is spend
that tracks *value*, not *activity*.

## Adapting it

- Set the ceiling to your runtime's real shared limits (rate limits, token budgets, concurrent-
  agent caps). One number, stated once.
- No subagents in your runtime? The inline-vs-spawn section is moot - keep context hygiene and the
  journal rule; they still pay.
- Map the tiers to the models you actually have (frontier / mid / small-or-local), per the
  "Any model" section of the method.
