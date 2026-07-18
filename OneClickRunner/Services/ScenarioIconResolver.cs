using System;
using System.IO;
using OneClickRunner.Models;

namespace OneClickRunner.Services;

/// <summary>An icon source for a Jump List task: a file that carries an icon, plus the icon index.</summary>
public readonly record struct ScenarioIcon(string Path, int Index);

/// <summary>
/// Picks a non-blank Jump List icon per scenario. Executables use their own icon; scripts borrow
/// their interpreter's icon (PowerShell, cmd, Python, WScript); anything unresolved falls back to the
/// app's own icon so no task ever appears blank.
/// </summary>
public static class ScenarioIconResolver
{
    public static ScenarioIcon Resolve(AppItem item)
    {
        if (item.Type == ScenarioType.YtDlp || item.Path == ScenarioLauncher.LegacyYtDlpSentinel)
        {
            return AppFallback();
        }

        var path = item.Path;
        if (string.IsNullOrWhiteSpace(path))
        {
            return AppFallback();
        }

        switch (Path.GetExtension(path).ToLowerInvariant())
        {
            case ".exe":
            case ".com":
            case ".scr":
                return File.Exists(path) ? new ScenarioIcon(path, 0) : AppFallback();

            case ".ps1":
                return InterpreterIcon(ScriptInterpreter.PowerShellHost());
            case ".bat":
            case ".cmd":
                return InterpreterIcon("cmd.exe");
            case ".py":
            case ".pyw":
                return InterpreterIcon("py.exe", "python.exe");
            case ".vbs":
            case ".vbe":
            case ".js":
                return InterpreterIcon("wscript.exe");

            default:
                // Some other file type: use its own icon if it exists, otherwise the app's.
                return File.Exists(path) ? new ScenarioIcon(path, 0) : AppFallback();
        }
    }

    /// <summary>
    /// First resolvable interpreter's icon, or the app fallback if none resolve. A candidate may be
    /// a bare command to look up on PATH or an already-resolved full path.
    /// </summary>
    private static ScenarioIcon InterpreterIcon(params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var resolved = Path.IsPathRooted(candidate)
                ? (File.Exists(candidate) ? candidate : null)
                : ScenarioLauncher.ResolveOnPath(candidate);
            if (resolved != null)
            {
                return new ScenarioIcon(resolved, 0);
            }
        }
        return AppFallback();
    }

    private static ScenarioIcon AppFallback()
    {
        var exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        return new ScenarioIcon(exe ?? string.Empty, 0);
    }
}
