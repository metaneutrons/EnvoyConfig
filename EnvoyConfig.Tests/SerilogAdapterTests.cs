using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace EnvoyConfig.Tests;

public class SerilogAdapterTests
{
    [Fact]
    public void Logs_Information_Message()
    {
        var events = new List<LogEvent>();
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Sink(new CollectingSink(events))
            .CreateLogger();

        logger.Information("Test message");

        Assert.Contains(events, e => e.MessageTemplate.Text == "Test message" && e.Level == LogEventLevel.Information);
    }

    private class CollectingSink : ILogEventSink
    {
        private readonly List<LogEvent> _events;
        public CollectingSink(List<LogEvent> events) => this._events = events;
        public void Emit(LogEvent logEvent) => this._events.Add(logEvent);
    }
}
