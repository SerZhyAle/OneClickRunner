using System;

namespace OneClickRunner.Models;

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
}
