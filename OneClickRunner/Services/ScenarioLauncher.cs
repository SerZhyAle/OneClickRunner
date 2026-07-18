using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using OneClickRunner.Models;

namespace OneClickRunner.Services;

/// <summary>Outcome of a launch attempt. Callers decide how to surface a failure.</summary>
public sealed class LaunchResult
{
    public bool Success { get; private init; }
    /// <summary>True when the user deliberately cancelled (e.g. dismissed the yt-dlp prompt or a UAC prompt).</summary>
    public bool Cancelled { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static LaunchResult Ok() => new() { Success = true };
    public static LaunchResult UserCancelled() => new() { Success = false, Cancelled = true };
    public static LaunchResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}

/// <summary>
/// The single code path that turns an <see cref="AppItem"/> into a running process. Both the
/// Jump List / command line (App) and the in-window Run button (MainWindow) route through here,
/// so their behaviour cannot drift again. Elevation is decided solely by
/// <see cref="AppItem.RunAsAdmin"/>, regardless of launch path.
/// </summary>
public static class ScenarioLauncher
{
    /// <summary>Legacy magic path kept working for scenarios created before the yt-dlp scenario type existed.</summary>
    public const string LegacyYtDlpSentinel = "SPECIAL_YTDLP";

    /// <summary>
    /// Launch <paramref name="item"/>. <paramref name="promptForLink"/> is invoked (on the caller's
    /// thread, which must be the UI thread) when a yt-dlp scenario needs a link; it returns the link
    /// or null if the user cancelled. Executable scenarios never call it.
    /// </summary>
    public static LaunchResult Launch(AppItem item, Func<string?>? promptForLink = null)
    {
        if (item.Type == ScenarioType.YtDlp || item.Path == LegacyYtDlpSentinel)
        {
            return LaunchYtDlp(item, promptForLink);
        }

        if (!TryResolveTarget(item, out var validationError))
        {
            LoggingService.Log($"Launch blocked for '{item.Name}': {validationError}");
            return LaunchResult.Fail(validationError!);
        }

        try
        {
            // A script is handed to its interpreter rather than shell-executed: the shell can only
            // start what the machine has an association for, which .ps1 never has. See ScriptInterpreter.
            var isScript = ScriptInterpreter.TryResolve(item.Path, item.Arguments, out var interpreter, out var interpreterArgs);
            var startInfo = new ProcessStartInfo
            {
                FileName = isScript ? interpreter : item.Path,
                Arguments = isScript ? interpreterArgs : item.Arguments,
                UseShellExecute = true,
            };
            if (item.RunAsAdmin)
            {
                startInfo.Verb = "runas";
            }
            if (!string.IsNullOrWhiteSpace(item.WorkingDirectory))
            {
                startInfo.WorkingDirectory = item.WorkingDirectory;
            }

            var process = Process.Start(startInfo);
            LoggingService.Log(
                $"Launched '{item.Name}' (pid={process?.Id}, admin={item.RunAsAdmin}): {startInfo.FileName} {startInfo.Arguments}");
            return LaunchResult.Ok();
        }
        catch (Win32Exception wex) when (wex.NativeErrorCode == 1223) // ERROR_CANCELLED - user declined UAC
        {
            LoggingService.Log($"Elevation cancelled by user for '{item.Name}'");
            return LaunchResult.UserCancelled();
        }
        catch (Exception ex)
        {
            LoggingService.Log($"Launch error for '{item.Name}': {ex.Message}");
            return LaunchResult.Fail(ex.Message);
        }
    }

    private static LaunchResult LaunchYtDlp(AppItem item, Func<string?>? promptForLink)
    {
        if (promptForLink == null)
        {
            return LaunchResult.Fail("yt-dlp scenarios require a link prompt, which is unavailable in this context.");
        }

        var link = promptForLink();
        if (string.IsNullOrWhiteSpace(link))
        {
            LoggingService.Log("yt-dlp launch cancelled by user");
            return LaunchResult.UserCancelled();
        }
        link = link.Trim();

        // The link is untrusted input. A '"' (or control character) could break out of the quoting
        // below and let cmd parse the rest as a command, so reject those outright - a valid URL never
        // contains them.
        if (link.IndexOf('"') >= 0 || ContainsControlChar(link))
        {
            return LaunchResult.Fail("The link contains characters that are not allowed (quotes or control characters).");
        }

        if (ResolveOnPath("yt-dlp") == null)
        {
            return LaunchResult.Fail("yt-dlp was not found on PATH. Install it and ensure 'yt-dlp' is runnable from a terminal.");
        }

        var outputFolder = string.IsNullOrWhiteSpace(item.YtDlpOutputFolder)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
            : item.YtDlpOutputFolder;
        try
        {
            Directory.CreateDirectory(outputFolder);
        }
        catch (Exception ex)
        {
            return LaunchResult.Fail($"Cannot use output folder '{outputFolder}': {ex.Message}");
        }

        try
        {
            // Keep the persistent, watchable console window (cmd /k) but never put the raw link on a
            // command line: it travels in an environment variable and is referenced quoted, so cmd
            // treats '&', '|', '<', '>' inside it as literal text rather than shell operators.
            var format = string.IsNullOrWhiteSpace(item.YtDlpFormat) ? string.Empty : item.YtDlpFormat.Trim() + " ";
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k yt-dlp {format}\"%OCR_YTDLP_LINK%\"",
                UseShellExecute = false, // required to pass Environment; also gives us the console window
                WorkingDirectory = outputFolder,
            };
            startInfo.Environment["OCR_YTDLP_LINK"] = link;

            var process = Process.Start(startInfo);
            LoggingService.Log($"Started yt-dlp (pid={process?.Id}) in '{outputFolder}' for link: {link}");
            return LaunchResult.Ok();
        }
        catch (Exception ex)
        {
            LoggingService.Log($"yt-dlp launch error: {ex.Message}");
            return LaunchResult.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Verify the target is resolvable before starting, so a missing file yields a clear message
    /// instead of an opaque Win32 exception. Existing file, a URL, or a PATH-resolvable command all pass.
    /// </summary>
    private static bool TryResolveTarget(AppItem item, out string? error)
    {
        error = null;
        var path = item.Path?.Trim() ?? string.Empty;
        if (path.Length == 0)
        {
            error = "The scenario has no path set.";
            return false;
        }

        if (File.Exists(path))
        {
            return true;
        }

        // A URL is launchable by the shell even though it is not a file.
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return true;
        }

        // Relative to the scenario's working directory, if one is set.
        if (!string.IsNullOrWhiteSpace(item.WorkingDirectory))
        {
            try
            {
                if (File.Exists(Path.Combine(item.WorkingDirectory, path)))
                {
                    return true;
                }
            }
            catch { /* malformed working directory - fall through to the PATH check */ }
        }

        // A rooted path that does not exist is definitely broken.
        if (Path.IsPathRooted(path))
        {
            error = $"File not found: {path}";
            return false;
        }

        // A bare command (e.g. calc.exe) is fine if it resolves on PATH.
        if (ResolveOnPath(path) != null)
        {
            return true;
        }

        error = $"'{path}' was not found as a file or on PATH.";
        return false;
    }

    /// <summary>Resolve a bare command against PATH / PATHEXT. Returns the full path or null.</summary>
    internal static string? ResolveOnPath(string command)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
        {
            return null;
        }

        var hasExtension = Path.HasExtension(command);
        var extensions = hasExtension
            ? new[] { string.Empty }
            : (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.BAT;.CMD;.COM")
                .Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var ext in extensions)
            {
                try
                {
                    var candidate = Path.Combine(dir.Trim(), command + ext);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
                catch { /* invalid PATH entry - skip */ }
            }
        }
        return null;
    }

    private static bool ContainsControlChar(string value)
    {
        foreach (var c in value)
        {
            if (char.IsControl(c))
            {
                return true;
            }
        }
        return false;
    }
}
