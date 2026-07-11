using System;
using System.Windows;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace OneClickRunner.Services;

/// <summary>
/// Applies a light or dark colour dictionary app-wide, following the Windows apps theme
/// (<c>AppsUseLightTheme</c>), and re-applies live when the user switches the OS theme. A minimal
/// in-repo <see cref="ResourceDictionary"/> per the T0023 ADR - no external theming dependency.
/// </summary>
public static class ThemeService
{
    private const string LightColorsUri = "pack://application:,,,/Themes/LightColors.xaml";
    private const string DarkColorsUri = "pack://application:,,,/Themes/DarkColors.xaml";

    private static ResourceDictionary? _current;

    public static void Apply(Application app)
    {
        LoadColors(app);

        // React to the user switching the Windows light/dark setting while the app is running.
        SystemEvents.UserPreferenceChanged += (_, e) =>
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                app.Dispatcher.Invoke(() => LoadColors(app));
            }
        };
    }

    private static void LoadColors(Application app)
    {
        try
        {
            var uri = new Uri(IsSystemDark() ? DarkColorsUri : LightColorsUri, UriKind.Absolute);
            var colors = new ResourceDictionary { Source = uri };

            if (_current != null)
            {
                app.Resources.MergedDictionaries.Remove(_current);
            }
            // Insert the colour dictionary first so styles (added via Styles.xaml) resolve against it.
            app.Resources.MergedDictionaries.Insert(0, colors);
            _current = colors;
            LoggingService.Log($"Theme applied: {(IsSystemDark() ? "dark" : "light")}");
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Theme apply failed: {ex.Message}");
        }
    }

    private static bool IsSystemDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", false);
            // AppsUseLightTheme: 1 = light, 0 = dark. Absent (older Windows) => treat as light.
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int i && i == 0;
        }
        catch
        {
            return false;
        }
    }
}
