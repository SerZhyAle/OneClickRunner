# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

OneClickRunner is a single-project WPF app (.NET 8, `net8.0-windows`, WPF + WinForms enabled) that lets the user launch configured apps/scripts ("scenarios") from the Windows taskbar Jump List and a system-tray icon. There are no tests.

## SZA Unified Rules (canon)

This repo follows the **SZA Unified Rules** - the shared source of truth for conventions across every
SZA project. Consumption model: **reference** (link the canon, don't mirror it). Read the canon for the
universal rules; this file keeps only OneClickRunner's deltas.

- **Canon:** the `sza` Claude Code plugin, from `github.com/SerZhyAle/sza-unified-rules` (adoption stamped
  in `.sza-canon.json`) - start at `rules/NEW_PROJECT_CHECKLIST.md`,
  then `AI_USAGE.md`, `DEVELOPMENT.md`, `TESTING_AND_QA.md`, `GITHUB_INTERACTION.md`, and the **Overlay A**
  section of `PLATFORM_OVERLAYS.md`.
- **This repo's record:** `contrib/oneclickrunner.md` in the canon - overlay facts + every divergence.

**Overlay & shape - Overlay A (Windows desktop), single channel.** Ships through **exactly one**
distribution channel - **GitHub Release** (portable `OneClickRunner-<version>-win-x64.zip` + `.sha256`,
framework-dependent, needs the .NET 8 Desktop Runtime). No winget / MSIX / Store / installer, so **no
frozen anchor to reserve** (a GitHub-Release-only portable has no update-correlation id). Single edition,
single binary. `LICENSE` (MIT) and `CHANGELOG.md` (house `[Unreleased]` flow) are at root.

- **Build/release wall.** `build.ps1` is **BUILD** - a local convenience that publishes and copies the
  exe to the hardcoded `C:\GD\tc\SZA\_APP`; it **never tags**. `release.ps1` is **RELEASE** - the only
  script that creates a `v<version>` tag, builds the zip + sha256, pushes the tag, and runs
  `gh release create`. Use `-DryRun` to stage artifacts without publishing.
- **Version shape.** Zero-padded date tag `YY.MMdd.HHmm`, stamped at build from `DateTime.Now`
  (`OneClickRunner/OneClickRunner.csproj`, `ProductVersion`; `release.ps1` overrides it with the exact tag
  so the exe stamp, tag, and zip name all match) - a valid Overlay A padding choice.

## Build & Run

```bash
dotnet build -c Release            # build the solution
dotnet run --project OneClickRunner # run from source
```

`build.ps1` publishes a framework-dependent win-x64 build to `OneClickRunner\bin\publish` and copies the exe to a **hardcoded deploy path** (`C:\GD\tc\SZA\_APP`). That path is user-specific - edit `$destination` before running on another machine.

## Architecture

Startup and command routing live in `App.xaml.cs` (kept thin, ~240 lines); feature logic is in `Services/` (`ScenarioLauncher`, `PipeIpcService`, `JumpListService`, `TrayIconService`, `ThemeService`, `AutostartService`, `ConfigurationService`, `ScriptInterpreter`). Layering is `Windows/App (UI) -> Services -> Models`. The pieces below only make sense together.

**No `StartupUri` - startup is manual.** `App.xaml` deliberately declares no startup window. `App.OnStartup` decides at runtime whether to (a) forward a command to an already-running instance and exit, (b) execute a one-shot command and exit, (c) show the settings window, or (d) start minimized.

**Single-instance + named-pipe IPC.** A named `Mutex` (`OneClickRunner_SingleInstance_Mutex`) enforces one instance. A second launch (e.g. clicking a Jump List task) connects to the live instance over a named pipe (`OneClickRunner_Command_Pipe`), writes its command-line arg, and exits. The live instance's `PipeIpcService` runs the background listener and marshals each command onto the UI thread.

**Command protocol.** Both CLI args and pipe messages use the same strings, routed through `HandleCommandLineArgs`: `/run:{guid}`, `/settings`, `/exit`. `/run:{guid}` looks the scenario up by `AppItem.Id`.

**Two launch surfaces - Jump List and system tray.** Both list the same scenarios in the same order; `JumpListService` builds a `JumpTask` per scenario (`Arguments = /run:{id}`) plus Settings and Exit, and `TrayIconService` builds the equivalent tray menu. After any scenario change call `App.RefreshJumpList()` (delegates to `JumpListService.Rebuild()`) - `MainWindow.LoadAppItems` already does this.

**Scenario storage = one XML file per item.** `ConfigurationService` persists each `AppItem` as a separate `XmlSerializer` file named `{guid}.xml` under `%APPDATA%\OneClickRunner\Scenarios\` (not a single config file). This per-file design is what makes Import (`MainWindow.ImportButton_Click`) work - importing an `.xml` adds one scenario and assigns a fresh `Id`. An empty folder is seeded with a default "Calculator" scenario. State lives on disk, and `App` and `MainWindow` each hold their own `ConfigurationService`, so call `Reload()` to pick up external changes before reading.

**Elevation is unified - decided only by `RunAsAdmin`.** All launches route through `ScenarioLauncher.Launch`, which sets `Verb = "runas"` **iff** `AppItem.RunAsAdmin` is true, regardless of launch surface (Jump List, tray, CLI, or the in-window Run button). This is the T0001 fix - the earlier "Jump List always elevates" gotcha is gone; behaviour no longer depends on how a scenario is started.

**yt-dlp is a first-class scenario type.** A yt-dlp scenario prompts for the link at run time (`LinkInputDialog`) and runs `yt-dlp` in the chosen output folder. The old magic path `"SPECIAL_YTDLP"` survives only as `ScenarioLauncher.LegacyYtDlpSentinel` for back-compat with pre-existing scenarios; new special-casing belongs in `ScenarioLauncher`, not in a magic `Path`.

**Window lifecycle.** Closing the settings window does not exit - `Window_Closing` cancels and minimizes unless `_isExiting` is set (only the Exit button sets it, after confirmation). Real shutdown is `Application.Current.Shutdown()`.

**Logging.** `LoggingService` (static) appends verbosely to `%APPDATA%\OneClickRunner\activity.log` and swallows its own errors. It's the primary debugging tool since much logic runs in short-lived one-shot process invocations.

## Working method (repo deltas - universal rules live in the canon `AI_USAGE.md`)

**Communication.** Chat in Russian; code / comments / logs / commits in English (canon `AUTHOR.md`
§Language). Dry and terse - no filler, no trailing summaries the user can read from the diff.

**Autonomy.** Don't ask permission to read, search, build, or run. Flag real blockers up
front. Fix trivial issues silently; surface only decisions that change behaviour or
architecture. Argue a wrong instruction once, then obey.

**Research order (never guess).** 1) CLAUDE.md, 2) README.md, 3) the code - grep for the
symbol before reading whole files, 4) official .NET/WPF docs when version-specific. Never
invent a path, symbol, or API. `activity.log` (`%APPDATA%\OneClickRunner\`) is the primary
runtime-debugging source since much logic runs in short-lived one-shot processes.

**Skill routing.** `/quick` trivial edit · `/fix` narrow bug · `/verify` run-and-observe ·
`/research` investigate first · `/spec`→`/spec-tech`→`/spec-dev`→`/spec-check` for a feature
with real design decisions · `/git` commit grouping · `/review` code review.

**Spec/plan tickets.** One ticket = `PLAN/<ID>_<slug>.md`, `<ID>` = `T0001`+. Status header
is the first `**Status:**` line; status comes from the working tree, never the filename. Full
flow in doc/SPEC_LIFECYCLE.md.

**Project structure & build.** Source root `OneClickRunner/`. Scratch `temp/` (git-ignored).
Architecture `Windows/App (UI) -> Services -> Models` - respect the direction; keep entry
points (`App.xaml.cs`, window code-behind) thin, delegate to `Services/`. Build
`dotnet build -c Release`; run `dotnet run --project OneClickRunner`; no test project.

**Strict rules.** No writes to repo root (scratch/backups → `temp/`). File budget ~500 lines; the
largest code-behind is `MainWindow.xaml.cs` (~460) - split it, not into it, if it grows. Never modify
`bin/`, `obj/`, `.vs/`, `*.g.cs`. Follow existing naming.

**Logging & anti-slop.** Ship logs only through `LoggingService.Log` - no `Console.*` or
`Debug.WriteLine` on live paths. No empty/broad `catch` that only logs, no trivial comments, no
dead code left behind. Full list in doc/CODE_QUALITY.md.

**Post-change discipline.** Record `expected | actual` for every check. A change is done only
when its user-visible behaviour works end-to-end - a passing compile is a milestone, not the
deliverable. For a launcher scenario, "done" means the Jump List task actually launches it.
Match evidence to change type: doc/VALIDATION.md.

**Persistent memory.** Native per-project memory is active (Claude Code). Save only durable,
non-obvious facts (why a decision went a certain way, a recurring gotcha) - never anything
derivable from the repo or git. Discipline: doc/AGENT_MEMORY.md.
