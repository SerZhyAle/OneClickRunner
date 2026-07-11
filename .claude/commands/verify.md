# Verify - Run-and-Observe Sanity Check

> **GLOBAL DIRECTIVES (anti-bureaucracy):**
> 1. Dry technical prose, no filler.
> 2. Surface only what matters: PASS/FAIL + evidence. Do not edit specs or status.
> 3. Terse report: end with one line - verdict + evidence path.

Lightweight check that a change **actually works when run**, not just that it compiles.
Build (optional), run/launch, walk a minimal scenario, capture output/logs, report
PASS/FAIL with evidence. Read-only on specs and plans. Artifacts go to `temp/`.

This is the in-between tool: heavier than reading the diff, lighter than a full QA pass.
Use it after `/quick`, `/fix`, or `/spec-dev` to catch a trivial breakage early.

## Usage

```text
/verify                                  # default smoke: start the app/target, basic happy path, scan logs for errors
/verify <free text describing what to check>
/verify <ticket-id>                      # read the ticket's acceptance criteria as scenario hints (do not edit the ticket)
/verify --build                          # rebuild before running (default: skip build)
/verify --dry-run                        # author the scenario only, no execution
```

Adapt the *how* to your stack: a web app → drive a browser or hit endpoints; a CLI → run
it with representative args; a service → start it and probe; a mobile app → launch on a
device/emulator. The kit defines the *method*, not the driver.

## Process

**1 - Parse arguments.** A ticket-id-shaped token → read its acceptance criteria as hints
(read-only). Otherwise treat the text as the scenario. Empty → default smoke.

**2 - Pre-flight.** Confirm the run target is reachable (server up, device online, binary
present). If not, report the blocker and stop - do not fake a pass.

**3 - Build + install (only when `--build`).** Run `dotnet build -c Release`. On failure: capture the
tail of the output to `temp/verify_<TS>.md` and abort. Do not proceed.

**4 - Author the scenario.** Write `temp/verify_<TS>.md` with a header (target,
version, environment) and an ordered 1–5 step scenario. Each step: `goal`, `action`,
`expected observable result`, optional `expected log line`. Sources, in priority order:
user free-text → ticket acceptance criteria → default smoke (start, exercise the main
path, assert no error/crash).

If `--dry-run`, stop here and report the path.

**5 - Capture output.** Start log/stdout capture before the run; record the start time.

**6 - Execute the scenario.** For each step: perform the action, capture evidence
(screenshot/response/stdout), verify the expected result, append a row to the run-log
table. On a crash/fatal error: capture the stack, mark FAIL, stop the run.

**7 - Analyse output.** Scan captured logs for error-level lines and exceptions. For a
ticket awaiting manual test, additionally grep for its verification tag (`<ID>:`) - each
hit means that code path was exercised. Append a findings section: counts per level, top
errors with references.

**8 - Report.** One line:
`verify: <target>, PASS/FAIL N/N, log errors N, crashes K. Scenario: temp/verify_<TS>.md`
Optional next step: all PASS → "OK to commit." Any FAIL → one-sentence root-cause guess +
route to `/fix` or `/spec-fix`.

## Constraints

- **Read-only on specs/plans/status.** Never flip a ticket status from here.
- **No commits.** `git status`/`git diff` may be inspected; committing is the user's call.
- **Outputs land only in `temp/`.** Never write to the repo root.
- **Never read a huge log fully into context** - tail or grep it (e.g. `tail -n 200` /
  `Get-Content -Tail 200`, or filter by level) and quote line numbers.
- **Re-resolve UI elements before each interaction** - never hardcode coordinates.
