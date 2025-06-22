using NLog;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace EnvoyConfig.Tests;

public class NLogAdapter
{
    [Fact]
    public void Logs_Information_Message()
    {
        var config = new LoggingConfiguration();
        var memoryTarget = new MemoryTarget { Layout = "${message}" };
        config.AddRule(LogLevel.Info, LogLevel.Fatal, memoryTarget);
        LogManager.Configuration = config;
        var logger = LogManager.GetLogger("TestLogger");

        logger.Info("Test message");

        Assert.Contains("Test message", memoryTarget.Logs);
    }
}
