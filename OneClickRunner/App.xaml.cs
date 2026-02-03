using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Shell;
using OneClickRunner.Services;
using Application = System.Windows.Application;

namespace OneClickRunner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static Mutex? _mutex;
    private const string MutexName = "OneClickRunner_SingleInstance_Mutex";
    private const string PipeName = "OneClickRunner_Command_Pipe";
    
    private MainWindow? _mainWindow;
    private ConfigurationService? _configService;
    private System.IO.Pipes.NamedPipeServerStream? _pipeServer;
    private System.Threading.Tasks.Task? _pipeListenerTask;

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
        else if (command == "/exit")
        {
            LoggingService.Log("Exit command handled, shutting down");
            System.Windows.Application.Current.Shutdown();
            return;
        }
        else if (command.StartsWith("/run:"))
        {
            var idString = command.Substring(5);
            LoggingService.Log($"Run command with ID: {idString}");
            
            if (Guid.TryParse(idString, out var id))
            {
                LoggingService.Log($"Parsed GUID: {id}");
                var items = _configService.GetAllItems();
                LoggingService.Log($"Total items: {items.Count}");
                
                var item = items.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    LoggingService.Log($"Found item: {item.Name}");
                    LoggingService.Log($"Item Path: {item.Path}");
                    LoggingService.Log($"Item Arguments: {item.Arguments}");
                    
                    try
                    {
                        // Special handling for yt-dlp command
                        if (item.Path == "SPECIAL_YTDLP")
                        {
                            LoggingService.Log("Special yt-dlp command detected, showing link input dialog");
                            
                            // Show dialog on UI thread
                            Current.Dispatcher.Invoke(() =>
                            {
                                var dialog = new OneClickRunner.Windows.LinkInputDialog();
                                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Link))
                                {
                                    var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                                    var command = $"cd /d \"{downloadsPath}\" && yt-dlp {dialog.Link}";
                                    
                                    var startInfo = new System.Diagnostics.ProcessStartInfo
                                    {
                                        FileName = "cmd.exe",
                                        Arguments = $"/k \"{command}\"",
                                        UseShellExecute = true,
                                        Verb = "" // No admin required
                                    };
                                    
                                    LoggingService.Log($"Starting yt-dlp with link: {dialog.Link}");
                                    var process = System.Diagnostics.Process.Start(startInfo);
                                    LoggingService.Log($"yt-dlp process started with ID: {process?.Id}");
                                }
                                else
                                {
                                    LoggingService.Log("yt-dlp command canceled by user");
                                }
                            });
                            return;
                        }
                        
                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = item.Path,
                            Arguments = item.Arguments,
                            UseShellExecute = true,
                            Verb = "runas" // Request admin privileges
                        };
                        if (!string.IsNullOrWhiteSpace(item.WorkingDirectory))
                        {
                            startInfo.WorkingDirectory = item.WorkingDirectory;
                            LoggingService.Log($"Working Directory: {item.WorkingDirectory}");
                        }
                        
                        LoggingService.Log($"Starting process with admin privileges: {item.Path} {item.Arguments}");
                        var process = System.Diagnostics.Process.Start(startInfo);
                        LoggingService.Log($"Process started with ID: {process?.Id}");
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Log($"Error starting process: {ex.Message}\n{ex.StackTrace}");
                    }
                }
                else
                {
                    LoggingService.Log($"Item with ID {id} not found");
                }
            }
            else
            {
                LoggingService.Log($"Failed to parse GUID from: {idString}");
            }
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            LoggingService.Log("Application started");
            LoggingService.Log($"Command line args: {string.Join(", ", e.Args)}");

            // Check if another instance is already running
            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);
            
            if (!createdNew)
            {
                LoggingService.Log("Another instance is already running - sending command via pipe");
                
                // Send command to existing instance via named pipe
                if (e.Args.Length > 0)
                {
                    try
                    {
                        using var pipeClient = new System.IO.Pipes.NamedPipeClientStream(".", PipeName, System.IO.Pipes.PipeDirection.Out);
                        pipeClient.Connect(1000); // 1 second timeout
                        
                        var command = e.Args[0];
                        using var writer = new StreamWriter(pipeClient);
                        writer.WriteLine(command);
                        writer.Flush();
                        
                        LoggingService.Log($"Command sent to existing instance: {command}");
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Log($"Failed to send command to pipe: {ex.Message}");
                    }
                }
                
                Shutdown();
                return;
            }

            _configService = new ConfigurationService();

            // Handle command line arguments for running apps
            if (e.Args.Length > 0 && e.Args[0].StartsWith("/run:"))
            {
                LoggingService.Log($"Command line execution: {e.Args[0]}");
                HandleCommandLineArgs(e.Args);
                Shutdown();
                return;
            }
            else if (e.Args.Length > 0 && e.Args[0] == "/exit")
            {
                LoggingService.Log("Exit command received, shutting down");
                Shutdown();
                return;
            }

            // Create main window
            _mainWindow = new MainWindow();
            LoggingService.Log("Main window created");

            // Check if there are any items or /settings argument
            var items = _configService?.GetAllItems() ?? new List<Models.AppItem>();
            
            if (items.Count == 0 || (e.Args.Length > 0 && e.Args[0] == "/settings"))
            {
                // Show settings window if no items configured or /settings argument
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Show();
                LoggingService.Log("Settings window shown (no items or /settings argument)");
            }
            else
            {
                // Start minimized - only show in taskbar
                _mainWindow.WindowState = WindowState.Minimized;
                _mainWindow.Show();
                LoggingService.Log("Application started minimized");
            }

            // Build jump list
            BuildJumpList();
            LoggingService.Log("Jump list built");
            
            // Start listening for commands from other instances
            StartPipeListener();
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Error in OnStartup: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    private void StartPipeListener()
    {
        _pipeListenerTask = System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                while (true)
                {
                    using var pipeServer = new System.IO.Pipes.NamedPipeServerStream(PipeName, System.IO.Pipes.PipeDirection.In);
                    pipeServer.WaitForConnection();
                    
                    using var reader = new StreamReader(pipeServer);
                    var command = reader.ReadLine();
                    
                    if (!string.IsNullOrEmpty(command))
                    {
                        LoggingService.Log($"Received command from pipe: {command}");
                        
                        // Handle the command
                        Dispatcher.Invoke(() =>
                        {
                            if (command.StartsWith("/run:"))
                            {
                                HandleCommandLineArgs(new[] { command });
                            }
                            else if (command == "/settings")
                            {
                                if (_mainWindow != null)
                                {
                                    _mainWindow.WindowState = WindowState.Normal;
                                    _mainWindow.Activate();
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log($"Pipe listener error: {ex.Message}");
            }
        });
    }

    private void StartPipeServer()
    {
        try
        {
            LoggingService.Log("Starting named pipe server");
            _pipeServer = new System.IO.Pipes.NamedPipeServerStream(PipeName, System.IO.Pipes.PipeDirection.InOut, 1, System.IO.Pipes.PipeTransmissionMode.Message, System.IO.Pipes.PipeOptions.Asynchronous);
            
            _pipeListenerTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    _pipeServer?.WaitForConnection();
                    LoggingService.Log("Named pipe client connected");

                    using (var reader = new System.IO.StreamReader(_pipeServer))
                    using (var writer = new System.IO.StreamWriter(_pipeServer) { AutoFlush = true })
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            LoggingService.Log($"Received command from pipe: {line}");
                            // Handle commands received from the pipe
                            HandleCommandLineArgs(line.Split(' '));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Log($"Error in pipe listener task: {ex.Message}\n{ex.StackTrace}");
                }
            }, System.Threading.Tasks.TaskCreationOptions.LongRunning);
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Error starting named pipe server: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void BuildJumpList()
    {
        try
        {
            LoggingService.Log("Building jump list");
            var jumpList = new JumpList();
            jumpList.ShowFrequentCategory = false;
            jumpList.ShowRecentCategory = false;

            var items = _configService?.GetAllItems() ?? new List<Models.AppItem>();
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;

            LoggingService.Log($"Found {items.Count} items, exe path: {exePath}");

            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                LoggingService.Log("Exe path not found or invalid");
                return;
            }

            foreach (var item in items)
            {
                var jumpTask = new JumpTask
                {
                    Title = item.Name,
                    Description = $"Run {item.Name}",
                    ApplicationPath = exePath,
                    Arguments = $"/run:{item.Id}",
                    IconResourcePath = item.Path,
                    IconResourceIndex = 0
                };
                jumpList.JumpItems.Add(jumpTask);
                LoggingService.Log($"Added jump task: {item.Name}");
            }

            // Add Settings option to jump list
            var settingsTask = new JumpTask
            {
                Title = "Settings",
                Description = "Open OneClickRunner Settings",
                ApplicationPath = exePath,
                Arguments = "/settings"
            };
            jumpList.JumpItems.Add(settingsTask);
            LoggingService.Log("Added settings task");

            // Add Exit option to jump list
            var exitTask = new JumpTask
            {
                Title = "Exit",
                Description = "Close OneClickRunner",
                ApplicationPath = exePath,
                Arguments = "/exit"
            };
            jumpList.JumpItems.Add(exitTask);
            LoggingService.Log("Added exit task");

            JumpList.SetJumpList(Application.Current, jumpList);
            jumpList.Apply();
            LoggingService.Log("Jump list applied successfully");
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Error building jump list: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void RefreshJumpList()
    {
        BuildJumpList();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LoggingService.Log($"Application OnExit called with exit code: {e.ApplicationExitCode}");
        
        if (_pipeServer != null)
        {
            try
            {
                _pipeServer.Dispose();
            }
            catch { }
        }
        
        if (_mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            LoggingService.Log("Mutex released");
        }
        
        base.OnExit(e);
        LoggingService.Log("Application OnExit completed");
    }
}

