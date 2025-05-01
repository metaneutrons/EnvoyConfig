using EnvoyConfig.Internal;
using EnvoyConfig.Logging;

namespace EnvoyConfig;

/// <summary>
/// Central API for loading environment configuration objects.
/// </summary>
/// <summary>
/// Central API for loading environment configuration objects from environment variables using reflection.
/// </summary>
public static class EnvConfig
{
    /// <summary>
    /// Global static prefix prepended to all environment variable lookups.
    /// </summary>
    public static string GlobalPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Loads and populates a configuration object of type T from environment variables.
    /// </summary>
    /// <typeparam name="T">The configuration class type to load.</typeparam>
    /// <param name="logger">Optional logger for warnings and errors.</param>
    /// <returns>The populated configuration object.</returns>
    public static T Load<T>(IEnvLogSink? logger = null)
        where T : new() => ReflectionHelper.PopulateInstance<T>(logger, GlobalPrefix);
}
