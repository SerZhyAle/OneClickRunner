# What's New / Changelog

Versions use the `YY.MMdd.HHmm` format, stamped from the build date/time.
The `[Unreleased]` section accumulates during regular **builds**; at **release** time it moves into a
new versioned section with a date and becomes the "What's New" text for the GitHub Release.

Categories: `Added`, `Changed`, `Fixed`, `Removed`.

## [Unreleased]

## [26.0723.1719] - 2026-07-23

### Added
- **First public build.** OneClickRunner launches your apps and scripts ("scenarios") in one click from
  the Windows **taskbar Jump List** and a **system-tray icon** - both list the same scenarios in the same
  order.
- **Any kind of scenario.** Run an executable, a batch/PowerShell/Python/VBScript file (with optional
  arguments and a working directory), or a **yt-dlp download** that asks you for the link at run time.
- **Settings window** to add, import, export, edit, clone, remove, reorder, and search your scenarios.
- **Per-scenario admin.** Mark a scenario to run elevated; elevation is decided **only** by that flag,
  the same way from every launch surface - no surprise UAC prompts.
- **Light/dark theme** that follows the Windows apps theme, and optional **start with Windows**.

### Notes
- This is a **framework-dependent** build: the target PC needs the **.NET 8 Desktop Runtime** installed.
- Download `OneClickRunner-<version>-win-x64.zip`, unzip, and run `OneClickRunner.exe`.
