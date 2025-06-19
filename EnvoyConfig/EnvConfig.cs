using EnvoyConfig.Internal;
using EnvoyConfig.Logging;

namespace EnvoyConfig;

/// <summary>
/// Provides static methods for loading configuration from environment variables
/// or .env files into strongly typed classes using the [Env] attribute.
/// </summary>
public static class EnvConfig
{
    /// <summary>
    /// Global static prefix prepended to all environment variable lookups.
    /// </summary>
    public static string GlobalPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to throw an exception when a type conversion error occurs.
    /// Defaults to <c>false</c>, meaning errors are logged and default values are used.
    /// </summary>
    public static bool ThrowOnConversionError { get; set; } = false;

    /// <summary>
    /// Loads configuration data from environment variables (and optionally a .env file specified by UseDotEnv)
    /// into a new instance of the specified configuration class <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the configuration class to populate. Must have a parameterless constructor.</typeparam>
    /// <param name="logger">Optional logger for warnings and errors.</param>
    /// <returns>A new instance of <typeparamref name="T"/> populated with configuration values.</returns>
    /// <exception cref="InvalidOperationException">Thrown if mapping fails (e.g., type conversion error, missing required variable without a default).</exception>
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

    /// <summary>
    /// Saves the current configuration values to a .env file.
    /// </summary>
    /// <typeparam name="T">The type of the configuration class to serialize.</typeparam>
    /// <param name="instance">The configuration instance to save.</param>
    /// <param name="filePath">The path where the .env file should be saved.</param>
    /// <param name="logger">Optional logger for warnings and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown if instance or filePath is null.</exception>
    /// <exception cref="IOException">Thrown if file writing fails.</exception>
    /// <remarks>
    /// This method uses reflection to find properties marked with the <see cref="Attributes.EnvAttribute"/>.
    /// It generates environment variable assignments based on the current property values.
    /// The GlobalPrefix is automatically applied to all variable names.
    /// </remarks>
    /// <example>
    /// <code>
    /// var config = EnvConfig.Load&lt;AppSettings&gt;();
    /// EnvConfig.Save(config, "current-config.env");
    /// </code>
    /// </example>
    public static void Save<T>(T instance, string filePath, IEnvLogSink? logger = null)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        ReflectionHelper.SaveToFile(instance, filePath, GlobalPrefix, logger, useDefaults: false);
    }

    /// <summary>
    /// Saves a template .env file with default values and structure for the specified configuration class.
    /// </summary>
    /// <typeparam name="T">The type of the configuration class to generate defaults for. Must have a parameterless constructor.</typeparam>
    /// <param name="filePath">The path where the .env template file should be saved.</param>
    /// <param name="logger">Optional logger for warnings and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown if filePath is null.</exception>
    /// <exception cref="IOException">Thrown if file writing fails.</exception>
    /// <remarks>
    /// This method creates a template .env file showing the structure and default values of the configuration class.
    /// Properties without defaults will have empty values, and nested lists will include single placeholder entries.
    /// The GlobalPrefix is automatically applied to all variable names.
    /// </remarks>
    /// <example>
    /// <code>
    /// EnvConfig.SaveDefaults&lt;AppSettings&gt;("config-template.env");
    /// </code>
    /// </example>
    public static void SaveDefaults<T>(string filePath, IEnvLogSink? logger = null)
        where T : new()
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        var defaultInstance = new T();
        ReflectionHelper.SaveToFile(defaultInstance, filePath, GlobalPrefix, logger, useDefaults: true);
    }
}
