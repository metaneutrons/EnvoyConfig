using EnvoyConfig.Attributes;
using EnvoyConfig.Logging;
using Xunit;

namespace EnvoyConfig.Tests;

using System;
using System.Collections.Generic;

public class Logging
{
    public class LogConfig
    {
        [Env(Key = "LOG_INT")]
        public int IntValue { get; set; }
    }

    private class CaptureLogSink : IEnvLogSink
    {
        public readonly List<(EnvLogLevel Level, string Msg)> Entries = [];

        public void Log(EnvLogLevel level, string message) => this.Entries.Add((level, message));
    }

    [Fact]
    public void Logs_Warnings_And_Errors()
    {
        var log = new CaptureLogSink();
        Environment.SetEnvironmentVariable("LOG_INT", "not-an-int");
        var config = EnvConfig.Load<LogConfig>(logger: log);
        Assert.Contains(log.Entries, e => e.Level == EnvLogLevel.Error && e.Msg.Contains("LOG_INT"));
    }
}
