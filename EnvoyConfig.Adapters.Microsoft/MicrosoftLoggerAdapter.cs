using EnvoyConfig.Logging;
using Microsoft.Extensions.Logging;

namespace EnvoyConfig.Adapters.Microsoft
{
    public class MicrosoftLoggerAdapter : IEnvLogSink
    {
        private readonly ILogger _logger;

        public MicrosoftLoggerAdapter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Log(EnvLogLevel level, string message)
        {
            var melLevel = ConvertLevel(level);
            _logger.Log(melLevel, new EventId(), message, null, (state, ex) => state);
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
