using System;
using System.Windows;
using System.Windows.Threading;

namespace OneClickRunner.Windows;

/// <summary>
/// A small, topmost, self-dismissing toast used to report a Jump List / command-line launch
/// failure - the only user-visible trace for a windowless one-shot launch. Shown modally so the
/// one-shot process stays alive until the notice has been on screen.
/// </summary>
public partial class NotificationWindow : Window
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(7);
    private DispatcherTimer? _timer;

    private NotificationWindow(string title, string message)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Show a transient failure notice. When <paramref name="blockUntilClosed"/> is true (the
    /// windowless one-shot /run process) it is shown modally so the process stays alive until the
    /// notice has been on screen; otherwise (a live instance) it is shown non-modally so it does not
    /// freeze the settings window.
    /// </summary>
    public static void ShowTransient(string title, string message, bool blockUntilClosed)
    {
        var window = new NotificationWindow(title, message);
        if (blockUntilClosed)
        {
            window.ShowDialog();
        }
        else
        {
            window.Show();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Bottom-right of the working area (above the taskbar), toast-style.
        var area = SystemParameters.WorkArea;
        Left = area.Right - Width - 16;
        Top = area.Bottom - ActualHeight - 16;

        _timer = new DispatcherTimer { Interval = Lifetime };
        _timer.Tick += (_, _) => Close();
        _timer.Start();
    }

    private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        _timer?.Stop();
        base.OnClosed(e);
    }
}
