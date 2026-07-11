using System.Threading;
using System.Windows;
using OneClickRunner.Services;
using Application = System.Windows.Application;

namespace OneClickRunner;

/// <summary>
/// Interaction logic for App.xaml. A thin orchestrator: it owns single-instance startup and routes
/// commands, delegating Jump List building, pipe IPC, scenario launching and the tray to services.
/// </summary>
public partial class App : Application
{
    private static Mutex? _mutex;
    private static bool _ownsMutex;
    private const string MutexName = "OneClickRunner_SingleInstance_Mutex";
    private const string PipeName = "OneClickRunner_Command_Pipe";

    private MainWindow? _mainWindow;
    private ConfigurationService? _configService;
    private JumpListService? _jumpListService;
    private TrayIconService? _trayIcon;
    private PipeIpcService? _pipeService;

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            LoggingService.Log("Application started");
            LoggingService.Log($"Command line args: {string.Join(", ", e.Args)}");

            // Apply the light/dark theme before any window is shown.
            ThemeService.Apply(this);

            _mutex = new Mutex(true, MutexName, out var createdNew);
            _ownsMutex = createdNew;
            if (!createdNew)
            {
                LoggingService.Log("Another instance is already running - forwarding command and exiting");
                // Run the requested command, or (plain relaunch) surface the existing window, so a
                // repeat launch is an obvious no-op instead of a second process.
                PipeIpcService.TrySend(PipeName, e.Args.Length > 0 ? e.Args[0] : "/settings");
                // We do NOT own the mutex here, so we must not release it - just close our handle.
                _mutex.Dispose();
                _mutex = null;
                Shutdown();
                return;
            }

            _configService = new ConfigurationService();

            // One-shot command paths: run or exit, then leave.
            if (e.Args.Length > 0 && e.Args[0].StartsWith("/run:"))
            {
                LoggingService.Log($"Command line execution: {e.Args[0]}");
                HandleCommandLineArgs(e.Args);
                Shutdown();
                return;
            }
            if (e.Args.Length > 0 && e.Args[0] == "/exit")
            {
                LoggingService.Log("Exit command received, shutting down");
                Shutdown();
                return;
            }

            _mainWindow = new MainWindow();
            LoggingService.Log("Main window created");

            var items = _configService.GetAllItems();
            if (items.Count == 0 || (e.Args.Length > 0 && e.Args[0] == "/settings"))
            {
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Show();
                LoggingService.Log("Settings window shown (no items or /settings argument)");
            }
            else
            {
                _mainWindow.WindowState = WindowState.Minimized;
                _mainWindow.Show();
                LoggingService.Log("Application started minimized");
            }

            _jumpListService = new JumpListService(
                () => _configService?.GetAllItems() ?? new List<Models.AppItem>());
            _jumpListService.Rebuild();
            LoggingService.Log("Jump list built");

            // Tray icon: a persistent home mirroring the Jump List's scenarios + Settings/Exit.
            _trayIcon = new TrayIconService(
                getItems: () => _configService?.GetAllItems() ?? new List<Models.AppItem>(),
                runItem: LaunchScenarioWithFeedback,
                showSettings: ShowSettingsWindow,
                exit: () => _mainWindow?.RequestExit());

            _pipeService = new PipeIpcService(PipeName, Dispatcher, DispatchPipeCommand);
            _pipeService.Start();
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Error in OnStartup: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    /// <summary>Route a CLI arg or pipe command: <c>/settings</c>, <c>/exit</c>, or <c>/run:{guid}</c>.</summary>
    private void HandleCommandLineArgs(string[] args)
    {
        if (_configService == null)
        {
            LoggingService.Log("HandleCommandLineArgs: _configService is null");
            return;
        }
        if (args.Length == 0)
        {
            LoggingService.Log("HandleCommandLineArgs: no arguments provided");
            return;
        }

        var command = args[0];
        LoggingService.Log($"HandleCommandLineArgs processing: {command}");

        if (command == "/settings")
        {
            LoggingService.Log("Settings command handled");
            return;
        }
        if (command == "/exit")
        {
            LoggingService.Log("Exit command handled, shutting down");
            System.Windows.Application.Current.Shutdown();
            return;
        }
        if (!command.StartsWith("/run:"))
        {
            return;
        }

        var idString = command.Substring(5);
        if (!Guid.TryParse(idString, out var id))
        {
            LoggingService.Log($"Failed to parse GUID from: {idString}");
            return;
        }

        _configService.Reload();
        var item = _configService.GetAllItems().FirstOrDefault(i => i.Id == id);
        if (item == null)
        {
            LoggingService.Log($"Item with ID {id} not found");
            return;
        }

        LoggingService.Log($"Found item: {item.Name} (Path: {item.Path})");
        // Single launch path shared with the Run button and the tray menu: elevation is decided
        // solely by item.RunAsAdmin, and the yt-dlp link is passed without a shell parsing it.
        LaunchScenarioWithFeedback(item);
    }

    private void DispatchPipeCommand(string command)
    {
        if (command.StartsWith("/run:"))
        {
            HandleCommandLineArgs(new[] { command });
        }
        else if (command == "/settings")
        {
            ShowSettingsWindow();
        }
        else if (command == "/exit")
        {
            HandleCommandLineArgs(new[] { command });
        }
    }

    /// <summary>
    /// Launch a scenario and report a non-cancelled failure as a transient toast. Shared by the
    /// Jump List / command line path and the tray menu (the Run button uses MainWindow's own path).
    /// </summary>
    private void LaunchScenarioWithFeedback(Models.AppItem item)
    {
        var result = ScenarioLauncher.Launch(item, () =>
        {
            var dialog = new OneClickRunner.Windows.LinkInputDialog();
            return dialog.ShowDialog() == true ? dialog.Link : null;
        });
        if (!result.Success && !result.Cancelled)
        {
            LoggingService.Log($"Launch failed for '{item.Name}': {result.ErrorMessage}");
            // Modal only in the one-shot /run process (no window of its own); in a live instance
            // show it non-modally so it never blocks the settings window.
            OneClickRunner.Windows.NotificationWindow.ShowTransient(
                $"Couldn't start “{item.Name}”",
                result.ErrorMessage ?? "The scenario could not be launched.",
                blockUntilClosed: _mainWindow == null);
        }
    }

    private void ShowSettingsWindow()
    {
        if (_mainWindow == null)
        {
            return;
        }
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    /// <summary>Rebuild both launcher surfaces from fresh config after a scenario change.</summary>
    public void RefreshJumpList()
    {
        _configService?.Reload();
        _jumpListService?.Rebuild();
        _trayIcon?.RebuildMenu();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LoggingService.Log($"Application OnExit called with exit code: {e.ApplicationExitCode}");

        _trayIcon?.Dispose();     // remove the tray icon (no ghost)
        _pipeService?.Dispose();  // stop the listener loop cleanly

        if (_mutex != null)
        {
            // Only the owning (first) instance may release; a second instance already disposed
            // its non-owned handle in OnStartup. Releasing a non-owned mutex throws.
            if (_ownsMutex)
            {
                _mutex.ReleaseMutex();
                LoggingService.Log("Mutex released");
            }
            _mutex.Dispose();
        }

        base.OnExit(e);
        LoggingService.Log("Application OnExit completed");
    }
}
