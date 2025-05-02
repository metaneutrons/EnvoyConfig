using EnvoyConfig.Logging;
using Serilog;

namespace EnvoyConfig.Adapters.Serilog
{
    using System;

    /// <summary>
    /// An adapter that implements <see cref="IEnvLogSink"/> for integration with Serilog logging.
    /// This adapter enables EnvoyConfig to log to Serilog by translating EnvoyConfig log levels to Serilog methods.
    /// </summary>
    public class SerilogLoggerAdapter : IEnvLogSink
    {
        /// <summary>
        /// The underlying Serilog <see cref="ILogger"/> instance used for logging.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogLoggerAdapter"/> class.
        /// </summary>
        /// <param name="logger">The Serilog <see cref="ILogger"/> instance to use for logging. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is null.</exception>
        public SerilogLoggerAdapter(ILogger logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logs a message to Serilog at the specified <see cref="EnvLogLevel"/>.
        /// </summary>
        /// <param name="level">The EnvoyConfig log level to use (Debug, Info, Warning, Error).</param>
        /// <param name="message">The message to log.</param>
        /// <remarks>
        /// The log level is mapped to the appropriate Serilog method:
        /// <list type="bullet">
        /// <item><description><see cref="EnvLogLevel.Debug"/> → <c>Debug</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Info"/> → <c>Information</c></description></item>
        /// <item><description><see cref="EnvLogLevel.Warning"/> → <c>Warning</c></description></item>
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
                    this._logger.Information(message);
                    break;
                case EnvLogLevel.Warning:
                    this._logger.Warning(message);
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
