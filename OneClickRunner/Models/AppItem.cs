using System;

namespace OneClickRunner.Models;

/// <summary>
/// The kind of action a scenario performs. Default is a plain executable/script launch;
/// <see cref="ScenarioType.YtDlp"/> prompts for a link and downloads it with yt-dlp.
/// </summary>
public enum ScenarioType
{
    Executable = 0,
    YtDlp = 1,
}

/// <summary>
/// Represents an application or script that can be run
/// </summary>
public class AppItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public bool RunAsAdmin { get; set; } = false;

    /// <summary>
    /// Explicit position in the settings list and Jump List. Lower comes first. Legacy
    /// scenarios deserialize with 0 and are assigned real ordinals on first load.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>Scenario kind. Absent in legacy XML => defaults to <see cref="ScenarioType.Executable"/>.</summary>
    public ScenarioType Type { get; set; } = ScenarioType.Executable;

    /// <summary>yt-dlp output folder. Empty => the user's Downloads folder.</summary>
    public string YtDlpOutputFolder { get; set; } = string.Empty;

    /// <summary>Optional extra yt-dlp arguments (e.g. <c>-f bestvideo+bestaudio</c>). Passed verbatim as discrete tokens.</summary>
    public string YtDlpFormat { get; set; } = string.Empty;

    /// <summary>
    /// A full copy of every field. Used by Edit (edit a copy, commit on OK) and Clone. Copying all
    /// fields - including Type, Order and the yt-dlp options - is required so those are never dropped.
    /// </summary>
    public AppItem Clone() => new()
    {
        Id = Id,
        Name = Name,
        Path = Path,
        Arguments = Arguments,
        WorkingDirectory = WorkingDirectory,
        Filename = Filename,
        RunAsAdmin = RunAsAdmin,
        Order = Order,
        Type = Type,
        YtDlpOutputFolder = YtDlpOutputFolder,
        YtDlpFormat = YtDlpFormat,
    };
}
