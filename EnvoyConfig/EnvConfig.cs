using System;
using EnvoyConfig.Internal;
using EnvoyConfig.Logging;

namespace EnvoyConfig;

/// <summary>
/// Provides static methods for loading configuration from environment variables
/// or .env files into strongly-typed classes using the [Env] attribute.
/// </summary>
public static class EnvConfig
{
    /// <summary>
    /// Global static prefix prepended to all environment variable lookups.
    /// </summary>
    public static string GlobalPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Loads configuration data from environment variables (and optionally a .env file specified by UseDotEnv)
    /// into a new instance of the specified configuration class <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the configuration class to populate. Must have a parameterless constructor.</typeparam>
    /// <param name="logger">Optional logger for warnings and errors.</param>
    /// <returns>A new instance of <typeparamref name="T"/> populated with configuration values.</returns>
    /// <exception cref="InvalidOperationException">Thrown if mapping fails (e.g., type conversion error, missing required variable without default).</exception>
    /// <remarks>
    /// This method uses reflection to find properties marked with the <see cref="Attributes.EnvAttribute"/>.
    /// It attempts to retrieve the corresponding environment variable and convert it to the property's type.
    /// See the documentation guides for details on supported types, default values, and nested classes.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class AppSettings
    /// {
    ///     [Env("APP_PORT")]
    ///     public int Port { get; set; }
    /// }
    ///
    /// var settings = EnvConfig.Load&lt;AppSettings&gt;();
    /// Console.WriteLine($"Application Port: {settings.Port}");
    /// </code>
    /// </example>
    public static T Load<T>(IEnvLogSink? logger = null)
        where T : new() => ReflectionHelper.PopulateInstance<T>(logger, GlobalPrefix);
}
