using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OneClickRunner.Models;

namespace OneClickRunner.Services;

/// <summary>
/// A system-tray presence backed by WinForms <see cref="NotifyIcon"/> (WinForms is already enabled,
/// so no new dependency). It is created on the WPF UI thread; WPF's dispatcher pumps the Win32
/// messages the icon needs. The menu is rebuilt from the same ordered scenario list as the Jump List
/// (call <see cref="RebuildMenu"/> from the shared refresh), and every action is routed through
/// caller-supplied callbacks so this service never depends on the UI layer.
/// </summary>
public sealed class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Func<IReadOnlyList<AppItem>> _getItems;
    private readonly Action<AppItem> _runItem;
    private readonly Action _showSettings;
    private readonly Action _exit;
    private bool _disposed;

    public TrayIconService(
        Func<IReadOnlyList<AppItem>> getItems,
        Action<AppItem> runItem,
        Action showSettings,
        Action exit)
    {
        _getItems = getItems;
        _runItem = runItem;
        _showSettings = showSettings;
        _exit = exit;

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Visible = true,
            Text = "OneClickRunner",
        };
        _notifyIcon.DoubleClick += (_, _) => _showSettings();
        RebuildMenu();
        LoggingService.Log("Tray icon created");
    }

    /// <summary>Rebuild the context menu from the current (ordered) scenario list plus Settings and Exit.</summary>
    public void RebuildMenu()
    {
        if (_disposed)
        {
            return;
        }

        var menu = new ContextMenuStrip();
        try
        {
            foreach (var item in _getItems())
            {
                var captured = item;
                var entry = new ToolStripMenuItem(item.Name);
                entry.Click += (_, _) => _runItem(captured);
                menu.Items.Add(entry);
            }
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Tray menu build error: {ex.Message}");
        }

        if (menu.Items.Count > 0)
        {
            menu.Items.Add(new ToolStripSeparator());
        }
        var settings = new ToolStripMenuItem("Settings");
        settings.Click += (_, _) => _showSettings();
        menu.Items.Add(settings);

        var exit = new ToolStripMenuItem("Exit");
        exit.Click += (_, _) => _exit();
        menu.Items.Add(exit);

        var old = _notifyIcon.ContextMenuStrip;
        _notifyIcon.ContextMenuStrip = menu;
        old?.Dispose();
    }

    private static System.Drawing.Icon LoadTrayIcon()
    {
        try
        {
            var resource = System.Windows.Application.GetResourceStream(
                new Uri("pack://application:,,,/Assets/app.ico"));
            if (resource != null)
            {
                using var stream = resource.Stream;
                return new System.Drawing.Icon(stream);
            }
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Tray icon load failed, using system default: {ex.Message}");
        }
        return System.Drawing.SystemIcons.Application;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
        LoggingService.Log("Tray icon disposed");
    }
}
