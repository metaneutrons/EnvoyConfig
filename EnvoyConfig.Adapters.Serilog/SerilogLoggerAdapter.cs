using EnvoyConfig.Logging;
using Serilog;

namespace EnvoyConfig.Adapters.Serilog
{
    using System;

    public class SerilogLoggerAdapter : IEnvLogSink
    {
        private readonly ILogger _logger;

        public SerilogLoggerAdapter(ILogger logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
