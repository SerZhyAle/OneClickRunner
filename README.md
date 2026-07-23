# OneClickRunner

Windows app to launch your apps and scripts ("scenarios") in one click from the Windows
taskbar Jump List.

**[Website](https://serzhyale.github.io/OneClickRunner/)** ·
**[Setup guide](https://serzhyale.github.io/OneClickRunner/guide.html)** ·
**[Source code](https://github.com/SerZhyAle/OneClickRunner)**

**[Download the latest release](https://github.com/SerZhyAle/OneClickRunner/releases/latest)** —
grab `OneClickRunner-<version>-win-x64.zip`, unzip, and run `OneClickRunner.exe`. The prebuilt
is framework-dependent, so the PC needs the **.NET 8 Desktop Runtime**. You can also build and run
from source with the .NET 8 SDK (below).

Part of the **[SZA](https://sza.od.ua)** family of tools — see also
[FastMediaSorter](https://serzhyale.github.io/FastMediaSorter_Lite/),
[CyrFlip](https://serzhyale.github.io/CyrFlip/),
[doc-html-translate](https://serzhyale.github.io/doc-html-translate/),
[FileDO](https://serzhyale.github.io/FileDO/) and
[Universal Agent Kit](https://serzhyale.github.io/universal-agent-kit/).

## Features

- **Two launch surfaces**: a taskbar **Jump List** (right-click the taskbar icon) and a
  **system tray icon** (right-click to run a scenario, double-click to open Settings). Both list
  the same scenarios in the same order.
- **Settings window**: add, import, export, edit, clone, remove, reorder (↑/↓), and search/filter
  scenarios. Right-click a row for a context menu; Enter runs, F2 edits, Del removes.
- **Flexible execution**: run any executable, batch, PowerShell, Python, or VBScript file,
  with optional arguments and working directory.
- **yt-dlp scenario type**: a first-class download scenario (pick an output folder and options;
  you are prompted for the link at run time) - no magic paths.
- **Per-scenario admin**: mark a scenario to run elevated. Elevation is decided **only** by that
  flag, the same way from every launch surface (no surprise UAC prompts).
- **Failure notice**: a Jump List launch that fails shows a brief on-screen notification.
- **Light/dark theme**: follows the Windows apps theme and updates when you switch it.
- **Windows autostart**: optionally start with Windows (runs minimized).

## How it works

- **The launcher UI is the Jump List and the tray icon** (the tray is the persistent home).
  Each scenario becomes a Jump List task and a tray-menu entry; activating one launches that
  scenario. Settings and Exit are offered on both.
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

- **Add** - create a scenario. Choose the type (executable/script, or yt-dlp download); for an
  executable set Name, Path, optional Arguments / Working Directory, and Run as Admin.
- **Import / Export** - bring a scenario in from, or save one out to, an `.xml` file.
- **Run** - launch the selected scenario (double-click, the Run button, Enter, or the context menu).
- **Edit / Clone / Remove** - modify, duplicate, or delete the selected scenario (F2 / Del work too).
- **Reorder** - the ↑ / ↓ buttons move the selected scenario; the order drives the Jump List and tray.
- **Search** - filter the list by name or path as you type.

Autostart is toggled in the Settings window ("Start OneClickRunner when Windows starts").

## Storage

Each scenario is stored as its own XML file under `%APPDATA%\OneClickRunner\Scenarios\`.
Activity is logged to `%APPDATA%\OneClickRunner\activity.log`.

## Requirements

- Windows 10 or 11
- .NET 8.0 SDK (to build and run from source)
