# Spec: T0024 - Portable single-file deploy (exe launches standalone)

**Ticket:** T0024 · **Proposal:** (field report) · **Status:** Verified · **Priority:** 88 · **Tier:** Quick Win · **Date:** 2026-07-11
**Complexity:** Primitive (build/deploy config, 1 file) - implement directly on approval, no tactical plan.

## Problem
The deployed `C:\GD\tc\SZA\_APP\OneClickRunner.exe` does not launch. `build.ps1` publishes a
**framework-dependent, multi-file** build (`--self-contained false`, no single-file) and then
copies **only** `OneClickRunner.exe` to the deploy folder. That `.exe` is a ~148 KB apphost stub
that cannot run without its siblings (`OneClickRunner.dll`, `OneClickRunner.runtimeconfig.json`,
`OneClickRunner.deps.json`) and the `runtimes/` subfolder - none of which are copied. Hence
"without subfolders, nothing launches."

Note: first-run folder provisioning is already correct and location-independent -
`ConfigurationService`/`LoggingService` create `%APPDATA%\OneClickRunner\Scenarios\` via
`Directory.CreateDirectory` on startup. The app simply never reached that code because the host
stub failed to start.

## Approach
- `build.ps1` - publish a **single-file** exe so the one deployed file is the whole app:
  `--self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true`
  (framework-dependent: target PC needs the .NET 8 Desktop Runtime, which is acceptable here).
  Clean `bin\publish` before publishing so no stale multi-file layout leaks into the deploy.
- Keep copying just `OneClickRunner.exe` (now self-sufficient); surface a note that the runtime
  must be present on the target.
- No app-code change: provisioning already handles missing folders on first run.

## Done criteria
- `bin\publish` after publish contains a single runnable `OneClickRunner.exe` (a few MB, no
  sibling `OneClickRunner.dll` / `runtimeconfig.json`).
- The exe copied alone into an empty folder launches (given .NET 8 Desktop Runtime installed) and
  creates `%APPDATA%\OneClickRunner\Scenarios\` on first run.

## Links
- Deploy-path counterpart to the launch-elevation work (T0001); independent of the other tickets.

**Result (2026-07-11):** Verified. `build.ps1` now publishes framework-dependent single-file
(`-p:PublishSingleFile=true`); `bin/publish` = one `OneClickRunner.exe` (~188 KB) + `.pdb`, no
sibling `.dll`/`.json`. Deployed to `C:\GD\tc\SZA\_APP` and launched: `activity.log` shows a
clean startup, exe path resolved to the deploy folder, Jump List built from 6 scenarios,
`%APPDATA%\OneClickRunner\Scenarios\` present.
