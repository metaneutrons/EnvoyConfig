namespace EnvoyConfig.Logging;

/// <summary>
/// Interface for logging from EnvConfig internals (warnings, errors, info).
/// </summary>
/// <summary>
/// Interface for logging from EnvConfig internals (warnings, errors, info).
/// </summary>
public interface IEnvLogSink
{
    /// <summary>
    /// Logs a message at the specified level.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    void Log(EnvLogLevel level, string message);
}

/// <summary>
/// Log levels for EnvConfig logging.
/// </summary>
public enum EnvLogLevel
{
    /// <summary>Trace messages (most verbose).</summary>
    Trace,

    /// <summary>Debug messages.</summary>
    Debug,

    /// <summary>Informational messages.</summary>
    Info,

    /// <summary>Warnings.</summary>
    Warning,

    /// <summary>Errors.</summary>
    Error,

    /// <summary>Logging disabled.</summary>
    Off,
}
