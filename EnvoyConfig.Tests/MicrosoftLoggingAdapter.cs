using Microsoft.Extensions.Logging;
using Xunit;

namespace EnvoyConfig.Tests;

using System;
using System.Collections.Generic;

public class MicrosoftLoggingAdapter
{
    [Fact]
    public void Logs_Information_Message()
    {
        var logs = new List<string>();
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logs));
        });
        var logger = loggerFactory.CreateLogger("TestLogger");

        logger.LogInformation("Test message");

        Assert.Contains(logs, l => l.Contains("Test message"));
    }

    private class TestLoggerProvider(List<string> logs) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TestLogger(logs);

        public void Dispose() { }
    }

    private class TestLogger : ILogger
    {
        private readonly List<string> _logs;

        public TestLogger(List<string> logs) => this._logs = logs;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            this._logs.Add(formatter(state, exception));
        }
    }
}
