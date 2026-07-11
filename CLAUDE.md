# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

OneClickRunner is a single-project WPF app (.NET 8, `net8.0-windows`, WPF + WinForms enabled) that lets the user launch configured apps/scripts ("scenarios") from the Windows taskbar Jump List. There are no tests.

## Build & Run

```bash
dotnet build -c Release            # build the solution
dotnet run --project OneClickRunner # run from source
```

`build.ps1` publishes a framework-dependent win-x64 build to `OneClickRunner\bin\publish` and copies the exe to a **hardcoded deploy path** (`C:\GD\tc\SZA\_APP`). That path is user-specific — edit `$destination` before running on another machine.

## Architecture

The whole app is driven from `App.xaml.cs`; the pieces below only make sense together.

**No `StartupUri` — startup is manual.** `App.xaml` deliberately declares no startup window. `App.OnStartup` decides at runtime whether to (a) forward a command to an already-running instance and exit, (b) execute a one-shot command and exit, (c) show the settings window, or (d) start minimized.

**Single-instance + named-pipe IPC.** A named `Mutex` (`OneClickRunner_SingleInstance_Mutex`) enforces one instance. A second launch (e.g. clicking a Jump List task) connects to the live instance over a named pipe (`OneClickRunner_Command_Pipe`), writes its command-line arg, and exits. The live instance runs a background pipe listener (`StartPipeListener`) that marshals the command onto the UI thread. Note `StartPipeServer` is dead/unused code — the active path is `StartPipeListener`.

**Command protocol.** Both CLI args and pipe messages use the same strings, routed through `HandleCommandLineArgs`: `/run:{guid}`, `/settings`, `/exit`. `/run:{guid}` looks the scenario up by `AppItem.Id`.

**Jump List is the launcher UI** (not a system tray, despite the README). `BuildJumpList` rebuilds a `JumpTask` per scenario (`Arguments = /run:{id}`) plus Settings and Exit tasks. Call `RefreshJumpList()` after any scenario change — `MainWindow.LoadAppItems` already does this.

**Scenario storage = one XML file per item.** `ConfigurationService` persists each `AppItem` as a separate `XmlSerializer` file named `{guid}.xml` under `%APPDATA%\OneClickRunner\Scenarios\` (not a single config file). This per-file design is what makes Import (`MainWindow.ImportButton_Click`) work — importing an `.xml` adds one scenario and assigns a fresh `Id`. An empty folder is seeded with a default "Calculator" scenario. State lives on disk, and `App` and `MainWindow` each hold their own `ConfigurationService`, so call `Reload()` to pick up external changes before reading.

**Elevation differs by launch path — a real gotcha.** Launching via Jump List / command line (`App.HandleCommandLineArgs`) **always** starts the process with `Verb = "runas"` (admin prompt). Launching via the in-window Run button (`MainWindow.RunButton_Click`) only elevates when `AppItem.RunAsAdmin` is true. The same scenario can therefore behave differently depending on how it's started.

**Special `SPECIAL_YTDLP` scenario.** An `AppItem.Path` of the sentinel `"SPECIAL_YTDLP"` is intercepted in `HandleCommandLineArgs`: it shows `LinkInputDialog`, then runs `yt-dlp <link>` in a `cmd /k` window `cd`'d to the user's Downloads folder. Any similar "magic path" special-casing goes in that same method.

**Window lifecycle.** Closing the settings window does not exit — `Window_Closing` cancels and minimizes unless `_isExiting` is set (only the Exit button sets it, after confirmation). Real shutdown is `Application.Current.Shutdown()`.

**Logging.** `LoggingService` (static) appends verbosely to `%APPDATA%\OneClickRunner\activity.log` and swallows its own errors. It's the primary debugging tool since much logic runs in short-lived one-shot process invocations.

## README caveat

`README.md` is stale: it describes a system tray and a single `config.json`. The implementation actually uses Windows Jump Lists and per-scenario XML files (above). Trust the code.

## Working method (adopted from Universal Agent Kit)

**Communication.** Chat in English; code / comments / logs / commits in English. Dry and
terse — no filler, no trailing summaries the user can read from the diff.

**Autonomy.** Don't ask permission to read, search, build, or run. Flag real blockers up
front. Fix trivial issues silently; surface only decisions that change behaviour or
architecture. Argue a wrong instruction once, then obey.

**Research order (never guess).** 1) CLAUDE.md, 2) README.md, 3) the code — grep for the
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
Architecture `Windows/App (UI) -> Services -> Models` — respect the direction; keep entry
points (`App.xaml.cs`, window code-behind) thin, delegate to `Services/`. Build
`dotnet build -c Release`; run `dotnet run --project OneClickRunner`; no test project.

**Strict rules.** No writes to repo root (scratch/backups → `temp/`). File budget ~500 lines
(`App.xaml.cs` is the one near it — split `HandleCommandLineArgs`/`BuildJumpList`/pipe
listener if it grows). Never modify `bin/`, `obj/`, `.vs/`, `*.g.cs`. Follow existing naming.

**Logging & anti-slop.** Ship logs only through `LoggingService.Log` — no `Console.*` or
`Debug.WriteLine` on live paths (note: `ConfigurationService` currently uses `Debug.WriteLine`
— clean up when you touch it). No empty/broad `catch` that only logs, no trivial comments, no
dead code left behind. Full list in doc/CODE_QUALITY.md.

**Post-change discipline.** Record `expected | actual` for every check. A change is done only
when its user-visible behaviour works end-to-end — a passing compile is a milestone, not the
deliverable. For a launcher scenario, "done" means the Jump List task actually launches it.
Match evidence to change type: doc/VALIDATION.md.

**Persistent memory.** Native per-project memory is active (Claude Code). Save only durable,
non-obvious facts (why a decision went a certain way, a recurring gotcha) — never anything
derivable from the repo or git. Discipline: doc/AGENT_MEMORY.md.
