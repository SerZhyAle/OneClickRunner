using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OneClickRunner.Services;

namespace OneClickRunner.Converters;

/// <summary>
/// Binds a scenario's <c>Path</c> to the icon embedded in (or associated with) that file, so the
/// list shows each scenario's real icon. Falls back to the app's own icon when the target has no
/// icon or cannot be resolved (e.g. a bare command not on PATH, or a yt-dlp scenario). Keeping this
/// in a converter avoids putting a non-serializable ImageSource on the AppItem model. Results are
/// cached by resolved path.
/// </summary>
public sealed class PathToIconConverter : IValueConverter
{
    private static readonly Dictionary<string, ImageSource?> _cache = new(StringComparer.OrdinalIgnoreCase);
    private static ImageSource? _appIcon;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var resolved = ResolveFile(value as string);
        if (resolved == null)
        {
            return AppIcon();
        }
        if (!_cache.TryGetValue(resolved, out var icon))
        {
            icon = Extract(resolved);
            _cache[resolved] = icon;
        }
        return icon ?? AppIcon();
    }

    private static string? ResolveFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }
        if (File.Exists(path))
        {
            return path;
        }
        // A bare command (e.g. calc.exe) or "yt-dlp" - use its icon if it resolves on PATH.
        return ScenarioLauncher.ResolveOnPath(path);
    }

    private static ImageSource? Extract(string file)
    {
        try
        {
            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(file);
            if (icon == null)
            {
                return null;
            }
            var source = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        catch
        {
            return null;
        }
    }

    private static ImageSource? AppIcon()
    {
        if (_appIcon != null)
        {
            return _appIcon;
        }
        try
        {
            var resource = System.Windows.Application.GetResourceStream(
                new Uri("pack://application:,,,/Assets/app.ico"));
            if (resource != null)
            {
                var decoder = new IconBitmapDecoder(
                    resource.Stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                _appIcon = decoder.Frames[0];
                _appIcon.Freeze();
            }
        }
        catch
        {
            // no icon available
        }
        return _appIcon;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
