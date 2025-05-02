using EnvoyConfig.Logging;
using Microsoft.Extensions.Logging;

namespace EnvoyConfig.Adapters.Microsoft
{
    using System;

    /// <summary>
    /// An adapter that implements <see cref="IEnvLogSink"/> for integration with Microsoft.Extensions.Logging.
    /// This adapter enables EnvoyConfig to log to Microsoft.Extensions.Logging by translating EnvoyConfig log levels to Microsoft log levels.
    /// </summary>
    public class MicrosoftLoggerAdapter : IEnvLogSink
    {
        /// <summary>
        /// The underlying Microsoft.Extensions.Logging <see cref="ILogger"/> instance used for logging.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftLoggerAdapter"/> class.
        /// </summary>
        /// <param name="logger">The Microsoft.Extensions.Logging <see cref="ILogger"/> instance to use for logging. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is null.</exception>
        public MicrosoftLoggerAdapter(ILogger logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logs a message to Microsoft.Extensions.Logging at the specified <see cref="EnvLogLevel"/>.
        /// </summary>
        /// <param name="level">The EnvoyConfig log level to use (Debug, Info, Warning, Error, Off).</param>
        /// <param name="message">The message to log.</param>
        /// <remarks>
        /// The log level is mapped to the corresponding Microsoft.Extensions.Logging <see cref="LogLevel"/>:
        /// <list type="bullet">
        /// <item><description><see cref="EnvLogLevel.Debug"/> → <c>LogLevel.Debug</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Info"/> → <c>LogLevel.Information</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Warning"/> → <c>LogLevel.Warning</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Error"/> → <c>LogLevel.Error</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Off"/> → <c>LogLevel.None</c></description></item>
        /// </list>
        /// </remarks>
        public void Log(EnvLogLevel level, string message)
        {
            var melLevel = ConvertLevel(level);
            this._logger.Log(melLevel, new EventId(), message, null, (state, ex) => state);
        }

        private static LogLevel ConvertLevel(EnvLogLevel level) =>
            level switch
            {
                EnvLogLevel.Debug => LogLevel.Debug,
                EnvLogLevel.Info => LogLevel.Information,
                EnvLogLevel.Warning => LogLevel.Warning,
                EnvLogLevel.Error => LogLevel.Error,
                EnvLogLevel.Off => LogLevel.None,
                _ => LogLevel.None,
            };
    }
}
