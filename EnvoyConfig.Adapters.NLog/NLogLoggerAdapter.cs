using EnvoyConfig.Logging;
using NLog;

namespace EnvoyConfig.Adapters.NLog
{
    using System;

    /// <summary>
    /// An adapter that implements <see cref="IEnvLogSink"/> for integration with NLog logging.
    /// This adapter enables EnvoyConfig to log to NLog by translating EnvoyConfig log levels to NLog methods.
    /// </summary>
    public class NLogLoggerAdapter : IEnvLogSink
    {
        /// <summary>
        /// The underlying NLog <see cref="Logger"/> instance used for logging.
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggerAdapter"/> class.
        /// </summary>
        /// <param name="logger">The NLog <see cref="Logger"/> instance to use for logging. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is null.</exception>
        public NLogLoggerAdapter(Logger logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logs a message to NLog at the specified <see cref="EnvLogLevel"/>.
        /// </summary>
        /// <param name="level">The EnvoyConfig log level to use (Debug, Info, Warning, Error).</param>
        /// <param name="message">The message to log.</param>
        /// <remarks>
        /// The log level is mapped to the appropriate NLog method:
        /// <list type="bullet">
        /// <item><description><see cref="EnvLogLevel.Debug"/> → <c>Debug</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Info"/> → <c>Info</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Warning"/> → <c>Warn</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Error"/> → <c>Error</c></description></item>
        /// </list>
        /// </remarks>
        public void Log(EnvLogLevel level, string message)
        {
            switch (level)
            {
                case EnvLogLevel.Debug:
                    this._logger.Debug(message);
                    break;
                case EnvLogLevel.Info:
                    this._logger.Info(message);
                    break;
                case EnvLogLevel.Warning:
                    this._logger.Warn(message);
                    break;
                case EnvLogLevel.Error:
                    this._logger.Error(message);
                    break;
                default:
                    break;
            }
        }
    }
}
