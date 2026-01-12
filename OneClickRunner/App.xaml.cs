using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using OneClickRunner.Models;
using OneClickRunner.Services;
using Application = System.Windows.Application;

namespace OneClickRunner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private NotifyIcon? _notifyIcon;
    private ConfigurationService? _configService;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _configService = new ConfigurationService();
        
        // Create system tray icon
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application, // Default icon for now
            Visible = true,
            Text = "OneClickRunner"
        };

        _notifyIcon.MouseClick += NotifyIcon_MouseClick;

        // Create main window but don't show it
        _mainWindow = new MainWindow();
        
        // Build context menu
        BuildContextMenu();
    }

    private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            // Left click - show settings window
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            // Right click - rebuild and show context menu
            BuildContextMenu();
        }
    }

    private void BuildContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        // Add app items
        var items = _configService?.GetAllItems() ?? new List<AppItem>();
        
        if (items.Count > 0)
        {
            foreach (var item in items)
            {
                var menuItem = new ToolStripMenuItem(item.Name);
                var appItem = item; // Capture for closure
                menuItem.Click += (s, e) => RunApplication(appItem);
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.Items.Add(new ToolStripSeparator());
        }

        // Add settings option
        var settingsItem = new ToolStripMenuItem("Settings");
        settingsItem.Click += (s, e) =>
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        };
        contextMenu.Items.Add(settingsItem);

        // Add exit option
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) =>
        {
            _notifyIcon!.Visible = false;
            _notifyIcon.Dispose();
            Shutdown();
        };
        contextMenu.Items.Add(exitItem);

        if (_notifyIcon != null)
        {
            _notifyIcon.ContextMenuStrip = contextMenu;
        }
    }

    private void RunApplication(AppItem appItem)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = appItem.Path,
                Arguments = appItem.Arguments,
                UseShellExecute = true
            };

            if (!string.IsNullOrWhiteSpace(appItem.WorkingDirectory))
            {
                startInfo.WorkingDirectory = appItem.WorkingDirectory;
            }

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to run '{appItem.Name}': {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
        base.OnExit(e);
    }
}

