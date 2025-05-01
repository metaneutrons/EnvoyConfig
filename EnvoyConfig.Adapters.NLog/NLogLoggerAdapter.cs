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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Log(EnvLogLevel level, string message)
        {
            switch (level)
            {
                case EnvLogLevel.Debug:
                    _logger.Debug(message);
                    break;
                case EnvLogLevel.Info:
                    _logger.Info(message);
                    break;
                case EnvLogLevel.Warning:
                    _logger.Warn(message);
                    break;
                case EnvLogLevel.Error:
                    _logger.Error(message);
                    break;
                default:
                    break;
            }
        }
    }
}
