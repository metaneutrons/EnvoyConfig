using EnvoyConfig.Logging;
using NLog;

namespace EnvoyConfig.Adapters.NLog
{
    using System;

    public class NLogLoggerAdapter : IEnvLogSink
    {
        private readonly Logger _logger;

        public NLogLoggerAdapter(Logger logger)
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
