using EnvoyConfig.Logging;
using Serilog;

namespace EnvoyConfig.Adapters.Serilog
{
    public class SerilogLoggerAdapter : IEnvLogSink
    {
        private readonly ILogger _logger;

        public SerilogLoggerAdapter(ILogger logger)
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
                    _logger.Information(message);
                    break;
                case EnvLogLevel.Warning:
                    _logger.Warning(message);
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
