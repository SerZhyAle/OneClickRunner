using System;
using System.IO;

namespace OneClickRunner.Services;

/// <summary>
/// Service for logging user activities
/// </summary>
public static class LoggingService
{
    private static readonly string _logPath;
    private static readonly object _lock = new object();

    static LoggingService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "OneClickRunner");
        Directory.CreateDirectory(appFolder);
        _logPath = Path.Combine(appFolder, "activity.log");
    }

    public static void Log(string message)
    {
        try
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"{timestamp} - {message}";
                File.AppendAllText(_logPath, logEntry + Environment.NewLine);
            }
        }
        catch
        {
            // Silently fail if logging fails
        }
    }
}