using System.Reflection;

namespace OneClickRunner.Services;

/// <summary>
/// Exposes the product version stamped at build time (YY.MMDD.HHmm from the build date).
/// </summary>
public static class VersionService
{
    /// <summary>The build-stamped version string, e.g. "26.0711.1421".</summary>
    public static string Version { get; } = ReadVersion();

    /// <summary>Version prefixed with "v", suitable for UI display.</summary>
    public static string DisplayVersion => "v" + Version;

    private static string ReadVersion()
    {
        var info = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        return string.IsNullOrWhiteSpace(info) ? "unknown" : info;
    }
}
