# OneClickRunner

Windows app to launch your apps and scripts ("scenarios") in one click from the Windows
taskbar Jump List.

**[Website](https://serzhyale.github.io/OneClickRunner/)** ·
**[Setup guide](https://serzhyale.github.io/OneClickRunner/guide.html)** ·
**[Releases](https://github.com/SerZhyAle/OneClickRunner/releases)**

Part of the **[SZA](https://sza.od.ua)** family of tools — see also
[FastMediaSorter](https://serzhyale.github.io/FastMediaSorter_Lite/),
[CyrFlip](https://serzhyale.github.io/CyrFlip/),
[doc-html-translate](https://serzhyale.github.io/doc-html-translate/),
[FileDO](https://serzhyale.github.io/FileDO/) and
[Universal Agent Kit](https://serzhyale.github.io/universal-agent-kit/).

## Features

- **Taskbar Jump List launcher**: right-click the OneClickRunner taskbar icon to run any
  configured scenario, or open Settings / Exit.
- **Settings window**: add, import, edit, clone, and remove scenarios.
- **Flexible execution**: run any executable, batch, PowerShell, Python, or VBScript file,
  with optional arguments and working directory.
- **Per-scenario admin**: mark a scenario to run elevated.
- **Windows autostart**: optionally start with Windows (runs minimized).

## How it works

- **The launcher UI is the Jump List**, not a system tray. Each scenario becomes a task on
  the taskbar Jump List (right-click the taskbar icon). Clicking a task launches that scenario.
- **Single instance**: one running instance handles everything; launching a Jump List task
  forwards the command to it over a named pipe.

## How to use

### Build

```bash
dotnet build -c Release
```

Run `OneClickRunner/bin/Release/net8.0-windows/OneClickRunner.exe`, or from source:

```bash
dotnet run --project OneClickRunner
```

### First run

The app opens the Settings window (on a brand-new install it seeds a sample "Calculator"
scenario). Add your own scenarios, then find them on the taskbar Jump List.

### Manage scenarios

- **Add** - create a scenario (Name, Path, optional Arguments / Working Directory, Run as Admin).
- **Import** - bring a scenario in from an `.xml` file.
- **Run** - launch the selected scenario (or double-click it).
- **Edit / Clone / Remove** - modify, duplicate, or delete the selected scenario.

Autostart is toggled in the Settings window ("Start OneClickRunner when Windows starts").

## Storage

Each scenario is stored as its own XML file under `%APPDATA%\OneClickRunner\Scenarios\`.
Activity is logged to `%APPDATA%\OneClickRunner\activity.log`.

## Requirements

- Windows 10 or 11
- .NET 8.0 Runtime (framework-dependent build)
