using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OneClickRunner.Services;

/// <summary>
/// Named-pipe IPC for the single-instance model: the live instance listens for commands from
/// secondary launches, and a secondary launch sends its command with <see cref="TrySend"/> before
/// exiting. Received commands are marshalled onto the UI thread and passed to the supplied handler.
/// The listen loop's failure boundary is per-connection, so one bad connection never stops it.
/// </summary>
public sealed class PipeIpcService : IDisposable
{
    private readonly string _pipeName;
    private readonly Dispatcher _dispatcher;
    private readonly Action<string> _onCommand;
    private CancellationTokenSource? _cts;
    private Task? _listenerTask;
    private bool _disposed;

    public PipeIpcService(string pipeName, Dispatcher dispatcher, Action<string> onCommand)
    {
        _pipeName = pipeName;
        _dispatcher = dispatcher;
        _onCommand = onCommand;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _listenerTask = Task.Run(async () =>
        {
            // Per-connection failure boundary: a failed accept/read is logged and the loop accepts
            // the next connection. Only cancellation (app shutdown) ends the loop.
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In);
                    await pipeServer.WaitForConnectionAsync(token);

                    using var reader = new StreamReader(pipeServer);
                    var command = await reader.ReadLineAsync(token);

                    if (!string.IsNullOrEmpty(command))
                    {
                        LoggingService.Log($"Received command from pipe: {command}");
                        _dispatcher.Invoke(() => _onCommand(command));
                    }
                }
                catch (OperationCanceledException)
                {
                    break; // clean shutdown
                }
                catch (Exception ex)
                {
                    LoggingService.Log($"Pipe connection error (listener continues): {ex.Message}");
                }
            }

            LoggingService.Log("Pipe listener stopped");
        }, token);
    }

    /// <summary>Send a single command to the running instance. Returns false if it could not be delivered.</summary>
    public static bool TrySend(string pipeName, string command, int timeoutMs = 1000)
    {
        try
        {
            using var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            pipeClient.Connect(timeoutMs);

            using var writer = new StreamWriter(pipeClient);
            writer.WriteLine(command);
            writer.Flush();

            LoggingService.Log($"Command sent to existing instance: {command}");
            return true;
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Failed to send command to pipe: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
