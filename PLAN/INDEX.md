# PLAN backlog - OneClickRunner

Strategic specs for proposed improvements. One ticket = one file `T00NN_<slug>.md`.
Status comes from the working tree, never the filename. Lifecycle:
`Draft -> Approved -> Tactical -> In Progress -> Implemented -> Verified` (see
[../doc/SPEC_LIFECYCLE.md](../doc/SPEC_LIFECYCLE.md)).

**Status (2026-07-11, branch `chore/import-agent-kit`): the entire backlog is implemented.** All 23
improvement tickets are **Implemented** with green Release builds after each; **T0024** (deploy fix)
is **Verified**. The whole set was built one ticket at a time with a Release build between each; the
refactored startup was smoke-tested at runtime (clean startup, theme applied, Jump List + tray built),
and the flagship elevation fix (T0001) was confirmed live (Calculator launched from the command line
with `admin=False`, no UAC prompt). Remaining step before "Verified" for the rest: interactive UI
exercise (`/verify`) and, when the user asks, deploy via `build.ps1` and commit.

## Backlog (by priority)

| ID | Prop | Title | Kind | Tier | Pri | Cx | Status |
|----|------|-------|------|------|----:|----|--------|
| [T0001](T0001_unify-launcher-elevation.md) | C1+D1 | Unify scenario launching; Jump List respects RunAsAdmin | bugfix | Moderate | 90 | Complex | Implemented |
| [T0024](T0024_portable-single-file-deploy.md) | field | Portable single-file deploy (exe launches standalone) | bugfix | Quick Win | 88 | Primitive | Verified |
| [T0002](T0002_resilient-pipe-listener.md) | C2 | Resilient named-pipe listener | bugfix | Easy | 85 | Complex | Implemented |
| [T0003](T0003_safe-ytdlp-invocation.md) | C3 | Safe yt-dlp invocation (no shell string injection) | bugfix/sec | Easy | 80 | Complex | Implemented |
| [T0004](T0004_validate-path-before-launch.md) | B3 | Validate scenario path before launch | bugfix | Easy | 70 | Primitive | Implemented |
| [T0005](T0005_manual-scenario-ordering.md) | A3 | Manual scenario ordering (drives Jump List order) | feature | Moderate | 65 | Complex | Implemented |
| [T0006](T0006_jumplist-launch-failure-notice.md) | B2 | Failure notification for Jump List launches | feature | Easy | 60 | Complex | Implemented |
| [T0007](T0007_double-click-run.md) | A1 | Double-click a row to run | feature | Quick Win | 55 | Primitive | Implemented |
| [T0008](T0008_export-scenario.md) | A4 | Export scenario to XML | feature | Easy | 55 | Primitive | Implemented |
| [T0009](T0009_system-tray-icon.md) | B6 | System tray icon | feature | Strategic | 55 | Complex | Implemented |
| [T0010](T0010_readme-accuracy.md) | D5 | Update README to match implementation | docs | Quick Win | 50 | Primitive | Implemented |
| [T0011](T0011_list-admin-workdir-columns.md) | A2 | Show RunAsAdmin + Working Dir in the list | feature | Easy | 50 | Primitive | Implemented |
| [T0012](T0012_rework-appitemdialog-layout.md) | A6 | Rework AppItemDialog layout; make resizable | feature | Moderate | 50 | Complex | Implemented |
| [T0013](T0013_ytdlp-scenario-type.md) | B4 | First-class yt-dlp scenario type | feature | Moderate | 50 | Complex | Implemented |
| [T0014](T0014_split-app-services.md) | D2 | Split App.xaml.cs into services | refactor | Moderate | 45 | Complex | Implemented |
| [T0015](T0015_clone-scenario.md) | A5 | Clone a scenario | feature | Quick Win | 45 | Primitive | Implemented |
| [T0016](T0016_context-menu-shortcuts.md) | A7 | Context menu + keyboard shortcuts | feature | Easy | 45 | Complex | Implemented |
| [T0017](T0017_autostart-processpath.md) | C4 | Autostart via Environment.ProcessPath | bugfix | Quick Win | 45 | Primitive | Implemented |
| [T0018](T0018_allow-zero-scenarios.md) | B5 | Allow zero scenarios (no forced Calculator reseed) | feature | Quick Win | 40 | Primitive | Implemented |
| [T0019](T0019_remove-dead-pipeserver.md) | D3 | Remove dead StartPipeServer method | chore | Quick Win | 40 | Primitive | Implemented |
| [T0020](T0020_configservice-logging-facade.md) | D4 | ConfigurationService logs via LoggingService | chore | Quick Win | 40 | Primitive | Implemented |
| [T0021](T0021_jumplist-script-icons.md) | B1 | Jump List icons for script scenarios | feature | Easy | 35 | Complex | Implemented |
| [T0022](T0022_list-search-filter.md) | A8 | Search / filter box in the list | feature | Easy | 35 | Complex | Implemented |
| [T0023](T0023_modern-theme-dark-mode.md) | A9 | Modern theme / dark mode | feature | Strategic | 30 | Complex | Implemented |

Legend: **Cx** = complexity gate. Primitive = <=3 files, no new types, implement directly.
Complex = auto-chains to `/spec-tech`. **Pri** 0..100 (bugfix 90, default 50).

## Notes
- **T0001 subsumed proposal D1.** The unified `ScenarioLauncher` is the mechanism that fixed the C1
  elevation inconsistency; they shipped together.
- **T0009 (tray) shipped, so T0010's premise changed.** The README should now mention the tray again
  (updated as part of this pass).
- **Service seams:** T0001/T0002 landed first, then T0014 moved their now-stable bodies plus the Jump
  List into `Services/JumpListService`, `Services/PipeIpcService`, `Services/ScenarioLauncher`.
- Traceability: each spec's `**Proposal:**` header maps back to the A/B/C/D catalogue.
