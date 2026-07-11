# Strategic Specification Writer

Write a **strategic** spec: the product-level *what* and *why*. No class names, file paths,
line budgets, or framework details - those belong to `/spec-tech`. The discipline of
keeping strategy free of implementation is the whole point: it forces you to agree on the
problem before arguing about the solution.

## Usage

```text
/spec <free-form feature description>      # default: natural language
/spec <slug> [--priority N]                # explicit short name
```

Never refuse input recognizable as a feature description. Only refuse genuinely unusable
input (empty, or a single content-free token like `?`/`help`) - print a short usage hint
and stop. Do not ask the user to pick between candidate slugs; choose one deterministically
and confirm it in the output.

Output file: `PLAN/<ID>_<slug>.md`. Allocate `<ID>` with **this repo's id scheme**
(`doc/SPEC_LIFECYCLE.md` "Adapting it"): a numeric scheme scans `PLAN/` for the highest
existing id and increments (e.g. `T0041` → `T0042`); an external-tracker (`JIRA-123`) or
date-slug scheme allocates the id that way instead. The tactical folder
`PLAN/<ID>_<slug>/` is created later by `/spec-tech`.

## Process

**1 - Parse and normalize.** Derive a kebab-case `slug` from the description (2–5
content-bearing words, ≤ 60 chars). Prefix `bugfix-` for fix/crash wording, `hotfix-` for
urgent/blocker wording. When the input is free-form, keep the user's original phrasing -
§1 must paraphrase *their* problem statement, not a reinterpretation. Default priority 50
(bugfix 90, hotfix 95); `--priority N` (0..100) overrides.

**2 - Read context.** The repo map, architecture doc, build/ops doc, and the existing
feature list. Enough to state the problem accurately and avoid duplicating an existing
capability.

**2.5 - Complexity gate (PRIMITIVE check).** Score the task:

- [ ] ≤ 3 existing files change - no new files
- [ ] No new classes, interfaces, or public types
- [ ] No schema/migration change
- [ ] No new dependency-injection wiring / module
- [ ] No new screen, route, or navigation destination
- [ ] Mechanically deterministic - no deferred design decisions
- [ ] Estimated delta < ~100 lines total

**If ALL pass → PRIMITIVE path** (skip steps 3–7):
1. Allocate the id, write a minimal spec (`## Problem`, `## Approach` - one bullet per file,
   `## Done criteria` - one observable check per file). Set `Status: In Progress`.
2. Implement directly, then run the narrowest meaningful check from `doc/VALIDATION.md` (a
   compile / type-check or a targeted test). A primitive still proves itself - do not set
   `Implemented` over a red or unrun check.
3. If acceptance includes manual testing, insert the verification tags (see §6 of
   `CLAUDE.md`) and set `Status: BlockNeedUserTest`. Otherwise `Status: Implemented`.
4. Report: `<ID> - Primitive. Implemented directly. Status: <status>.`

**If ANY criterion fails → COMPLEX path:** continue.

**3 - Determine tier.** A rough size label for triage: `Quick Win` · `Easy` · `Moderate` ·
`Strategic` · `Security/Compliance (urgent)`. Tier is a human-facing triage hint only - no skill
gates on it (`/backlog` selects by `Priority`), so keep it approximate.

**4 - Allocate the id** before any file write (per the repo's id scheme - scan-and-increment
for a numeric scheme; otherwise follow that scheme).

**5 - Write the strategic file** using the template below.

**5.1 - Owner inputs (Approval gate).** Fill §3.3 with only the bullets that match the
spec's actual scope - do not pad with `n/a` lines to look thorough. The one universally
required field is **Related tickets**. Emit a bullet only when its tag is triggered by the
slug or the §1/§3.2 text, for example:
- user-facing wording touched → **Copy/tone policy**
- data/schema touched → **Data compatibility**
- performance-critical → **Performance budget**
- platform/version-bound → **Platform constraints**
- localization touched → **Localization**
- any tag matched → also **Validation level** and **Owner sign-off**

**6 - Approve.** Flip `Status: Draft → Approved` in the file. (The style gate - lists over
tables, one idea per bullet, no section summaries - is enforced *here*, at approval, not while
drafting. A Draft may stay rough.)

**7 - Auto-chain to `/spec-tech`.** Immediately invoke `/spec-tech <ID>` unless a §6
research item is `Open` and marked as required before implementation - then list those and
ask whether to proceed.

**Chat output:** `<ID> <slug> - Tier T, Priority P. Status: Approved. -> Running /spec-tech..`

## Status lifecycle

The gate table - which status each skill requires and produces, the full flow, and every block
state - is defined once in `doc/SPEC_LIFECYCLE.md`; this skill does not restate it. This skill's
row: requires none (allocates an id), produces `Approved` (or `Implemented` / `BlockNeedUserTest`
for a primitive), auto-chains to `/spec-tech` for a complex spec. When `/spec` records a
dependency it sets `BlockByOtherTask` in §10; every transition into a `Block*` status carries a
one-line note - what must be tested, or what the blocker is and what resolves it.

## Template

```markdown
# Strategic spec: <ID> - <Feature name>

**Ticket:** <ID>
**Status:** Draft
**Priority:** <0..100>
**Date:** <YYYY-MM-DD>
**Tier:** <label>
**Tactical plan:** `PLAN/<ID>_<slug>/` (created by /spec-tech)

> **Scope:** STRATEGIC. Goals, constraints, open questions. No class names, paths, line
> budgets, schema versions, or framework module details.

---

## 1. Problem
<2-4 sentences. What is broken or missing? Effect on the user. Affected area (module /
feature, not class names).>

## 2. Goals
<Numbered list of observable improvements: "what becomes possible / what stops happening".>

**Non-goals:**
- <explicitly out of scope>

## 3. Wishes and constraints
### 3.1 Owner wishes
<Desired but not required for the first iteration.>

### 3.2 Hard constraints
- **Platform / versions:** <or "none">
- **Performance:** <budget if critical, else "n/a">
- **Data compatibility:** <migration shape, no version numbers>
- **Localization:** <required locales, or "n/a">
- **Accessibility:** <if the feature is user-facing>

### 3.3 Owner inputs (Approval gate)
<Only the bullets matching detected scope; fill concrete values, no placeholders.>
- **Related tickets:** <dependencies / dependents, or "none">

## 4. Current architecture context
<1-2 paragraphs: which layers/components own the affected area, and why the problem cannot
be solved as-is. No class names.>

## 5. Proposed approach
<Architectural level: which roles appear, what reads/writes where, what responsibility
shifts. No class/file/method names.>

### 5.1 Pillars / modules
<Major logical blocks, each with a goal and requirements.>

### 5.2 Data & event flows
<High-level: "UI -> application layer -> cache -> ..". No method names.>

### 5.3 Extension points
<What must stay open to extension.>

## 6. Open questions / research items
1. **<title>**
   - **Question:** <..>
   - **Options:** <if known>
   - **To find out:** <what to check>
   - **Status:** Open / Resolved
   - **Artifact:** `PLAN/<ID>_<slug>/research/<NN>__<topic>.md` <if resolved by real research>

<If none: "No open questions.">

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| <..> | Low/Med/High | <what breaks> | <how to prevent> |

## 8. User impact (docs)
<Default: "No changes to user docs." Only if this introduces a capability the user would
perceive as new: one sentence for the feature docs.>

## 9. Architecture decisions (ADR)
**ADR-1: <title>** - Decision / Alternatives / Why.
<If none: "No ADRs - decision follows established project patterns.">

## 10. Links to other specs
<List or "None.">

## 11. Done criteria (strategic)
<Numbered, observable outcomes - not architecture claims. "User sees X" / "Batch finishes
in under N minutes".>

## 12. Next step
`/spec-tech <ID>` - creates the phased tactical plan.
```

## Constraints

- §5: architectural roles only - no class names, file paths, line budgets, schema versions,
  DI modules.
- §11: observable outcomes only.
- §6 and §7 are mandatory even if trivial - write an explicit "none" rather than omitting.
- Do not duplicate an existing feature-doc entry.
- The style + completeness gate runs only at the `Draft -> Approved` flip; a Draft may keep
  rough phrasing.
