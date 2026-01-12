# OneClickRunner
Windows application to run anything by one click

## Features

- **System Tray Integration**: Runs minimized in the Windows system tray
- **Quick Access**: Right-click the tray icon to see and run your configured applications/scripts
- **Easy Configuration**: Left-click the tray icon to open settings
- **Windows Autostart**: Option to start automatically with Windows
- **Flexible Execution**: Run any executable, batch file, or PowerShell script

## How to Use

### Installation
1. Build the application using Visual Studio or .NET CLI:
   ```bash
   dotnet build -c Release
   ```
2. Run the executable from `bin/Release/net8.0-windows/OneClickRunner.exe`

### First Time Setup
1. The application will start minimized in the system tray (notification area)
2. Left-click the tray icon to open the settings window
3. Click "Add" to add your first application or script
4. Fill in the details:
   - **Name**: Display name for the application
   - **Path**: Full path to the executable or script
   - **Arguments** (optional): Command-line arguments
   - **Working Directory** (optional): Starting directory for the application
5. Enable "Start OneClickRunner when Windows starts" to run on startup

### Running Applications
1. Right-click the OneClickRunner icon in the system tray
2. Select the application or script you want to run from the menu
3. The application will start immediately

### Managing Applications
- **Add**: Click "Add" button to add new applications/scripts
- **Edit**: Select an item and click "Edit" to modify it
- **Remove**: Select an item and click "Remove" to delete it

## System Requirements
- Windows 11 (or Windows 10)
- .NET 8.0 Runtime

## Technical Details
- Built with WPF and .NET 8.0
- Configuration stored in: `%APPDATA%\OneClickRunner\config.json`
- Autostart configured via Windows Registry

