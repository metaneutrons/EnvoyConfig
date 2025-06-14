using System;
using EnvoyConfig.Logging;

namespace EnvoyConfig.Conversion
{
    /// <summary>
    /// Defines a contract for custom type converters.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Converts a string value to the specified target type.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <param name="targetType">The type to convert the value to.</param>
        /// <param name="logger">Optional logger for warnings and errors.</param>
        /// <returns>The converted object, or a default value if conversion fails.</returns>
        object? Convert(string? value, Type targetType, IEnvLogSink? logger);
    }
}
