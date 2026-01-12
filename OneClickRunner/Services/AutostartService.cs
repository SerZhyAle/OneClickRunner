using Microsoft.Win32;
using System;

namespace OneClickRunner.Services;

/// <summary>
/// Service for managing Windows autostart
/// </summary>
public class AutostartService
{
    private const string RunRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "OneClickRunner";

    public bool IsAutostartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
            var value = key?.GetValue(AppName);
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    public void EnableAutostart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exePath = exePath.Replace(".dll", ".exe");
            key?.SetValue(AppName, $"\"{exePath}\"");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to enable autostart", ex);
        }
    }

    public void DisableAutostart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            key?.DeleteValue(AppName, false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to disable autostart", ex);
        }
    }
}
