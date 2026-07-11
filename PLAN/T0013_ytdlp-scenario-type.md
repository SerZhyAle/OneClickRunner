# Strategic spec: T0013 - First-class yt-dlp scenario type

**Ticket:** T0013
**Proposal:** B4
**Status:** Implemented
**Priority:** 50
**Date:** 2026-07-11
**Tier:** Moderate
**Tactical plan:** `PLAN/T0013_ytdlp-scenario-type/` (created by /spec-tech)

## 1. Problem
The yt-dlp download is triggered by a magic `Path` sentinel (`"SPECIAL_YTDLP"`) special-cased
in the command handler. It is undiscoverable in the UI, hardcodes the Downloads folder and a
bare `yt-dlp` on `PATH`, offers no options, and does not check that yt-dlp is installed. Adding
any similar "action" scenario means another magic string.

## 2. Goals
1. yt-dlp is a selectable scenario **type** in the Add/Edit dialog, not a hidden sentinel.
2. The user can choose the output folder (default Downloads) and optionally basic format options.
3. Missing yt-dlp is detected and reported instead of silently failing.

**Non-goals:**
- A general plugin system for arbitrary action types (this is one concrete type; a generalized
  abstraction can follow if more types appear).

## 4. Current architecture context
The sentinel is interpreted in `App.HandleCommandLineArgs`; input is gathered by
`Windows/LinkInputDialog`. `AppItem` has no notion of a scenario "type" - it is always a path +
args. Safe invocation is addressed narrowly in T0003; this ticket makes it a real type.

## 5. Proposed approach
Model a scenario type on `AppItem` (default = plain executable, plus a yt-dlp type). The
Add/Edit dialog surfaces the type and its options; the launcher branches on type through the
shared launch path (T0001) using the safe invocation from T0003. Preserve backward compatibility
with any existing `SPECIAL_YTDLP` scenarios by mapping them to the new type on load.

### 5.1 Pillars
- Model: a scenario-type field (+ yt-dlp options: output folder, format).
- UI: type selector and conditional option fields in the dialog (built on T0012's layout).
- Launcher: type-aware dispatch reusing T0001 + T0003.
- Compatibility: migrate legacy `SPECIAL_YTDLP` scenarios.

## 6. Open questions
1. **Option scope** - how many yt-dlp options to expose (just output folder, or format/quality too).
2. **yt-dlp discovery** - PATH only, or allow pointing at an explicit yt-dlp path.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Legacy sentinel scenarios break | Medium | Existing yt-dlp task stops | Map `SPECIAL_YTDLP` to the new type on load |
| Serialized model change | Medium | Old XML missing the field | Default the type when absent (XmlSerializer tolerates missing elements) |

## 9. Architecture decisions
**ADR-1: typed scenario vs more sentinels** - Decision: an explicit type field. Alternatives:
keep magic paths. Why: discoverable, extensible, removes string-matching in the launcher.

## 10. Links to other specs
- Builds on T0003 (safe invocation), T0001 (shared launcher), T0012 (dialog layout).

## 11. Done criteria (strategic)
1. A user can create a yt-dlp scenario from the dialog without typing a magic path.
2. The output folder is user-selectable; default is Downloads.
3. Running with yt-dlp absent reports a clear error.
4. A pre-existing `SPECIAL_YTDLP` scenario still works after upgrade.

## 12. Next step
Done.

**Result (2026-07-11):** Implemented. `AppItem` gained `Type` (`ScenarioType` enum, default
`Executable`) plus `YtDlpOutputFolder` and `YtDlpFormat`. The Add/Edit dialog has a **Scenario type**
selector that toggles between the executable fields and yt-dlp option fields (download folder w/ Browse,
extra options), on the T0012 layout. `ScenarioLauncher` dispatches on type through the shared launch
path, using the T0003 safe invocation; **yt-dlp discovery** = PATH, with a clear error when absent
(done-criteria 3). **Option scope decision (open questions):** output folder + a free-form extra-options
box (keeps it simple, still flexible). Legacy `SPECIAL_YTDLP` scenarios keep working (launcher checks the
sentinel) and are surfaced as the yt-dlp type on load, so editing them is natural (done-criteria 4).
Release build: 0 errors.

**Review follow-up (2026-07-11, high-severity fix):** The adversarial review found that `Edit` and
`Clone` in `MainWindow` copied only the original `AppItem` fields, dropping the new `Type`,
`YtDlpOutputFolder`, `YtDlpFormat` (and `Order`, from T0005). Editing or cloning a yt-dlp scenario
therefore reopened it as an Executable and, on save, permanently converted it to a broken `yt-dlp`
executable launch (the load-time rescue only matches the legacy `SPECIAL_YTDLP` sentinel, not the new
`Path="yt-dlp"`). Fixed by adding `AppItem.Clone()` (copies every field) and using it in both
`EditButton_Click` and `CloneButton_Click`. Release build: 0 errors.
