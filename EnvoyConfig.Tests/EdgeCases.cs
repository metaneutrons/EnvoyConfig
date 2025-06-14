using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

using System;

public class EdgeCases
{
    public class EdgeConfig
    {
        [Env(Key = "EDGE_INT")]
        public int IntValue { get; set; }

        [Env(Key = "EDGE_BOOL")]
        public bool BoolValue { get; set; }
    }

    [Fact]
    public void Handles_Missing_And_Invalid_Values()
    {
        Environment.SetEnvironmentVariable("EDGE_INT", null);
        Environment.SetEnvironmentVariable("EDGE_BOOL", "notabool");
        var config = EnvConfig.Load<EdgeConfig>();
        Assert.Equal(0, config.IntValue); // default(int)
        Assert.False(config.BoolValue); // default(bool)
    }
}

public class StandardTypesConfig
{
    [Env("TEST_DATETIME")]
    public DateTime MyDateTime { get; set; }

    [Env("TEST_TIMESPAN")]
    public TimeSpan MyTimeSpan { get; set; }

    [Env("TEST_GUID")]
    public Guid MyGuid { get; set; }
}

public class StandardTypeConversionTests
{
    public StandardTypeConversionTests()
    {
        // Ensure errors don't throw for these tests, we want to check default value assignment
        EnvConfig.ThrowOnConversionError = false;
    }

    [Fact]
    public void DateTime_Valid()
    {
        var expectedDateTime = new DateTime(2023, 10, 27, 14, 30, 0);
        Environment.SetEnvironmentVariable("TEST_DATETIME", expectedDateTime.ToString("o")); // ISO 8601
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(expectedDateTime, config.MyDateTime);
    }

    [Fact]
    public void DateTime_Missing_ReturnsDefault()
    {
        Environment.SetEnvironmentVariable("TEST_DATETIME", null);
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(default(DateTime), config.MyDateTime);
    }

    [Fact]
    public void DateTime_Invalid_ReturnsDefault()
    {
        Environment.SetEnvironmentVariable("TEST_DATETIME", "not-a-datetime");
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(default(DateTime), config.MyDateTime);
    }

    [Fact]
    public void TimeSpan_Valid_HoursMinutesSeconds()
    {
        var expectedTimeSpan = new TimeSpan(14, 30, 0); // 14h 30m
        Environment.SetEnvironmentVariable("TEST_TIMESPAN", expectedTimeSpan.ToString("c")); // "hh:mm:ss"
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(expectedTimeSpan, config.MyTimeSpan);
    }

    [Fact]
    public void TimeSpan_Valid_DaysHoursMinutesSeconds()
    {
        var expectedTimeSpan = new TimeSpan(2, 14, 30, 0); // 2d 14h 30m
        Environment.SetEnvironmentVariable("TEST_TIMESPAN", expectedTimeSpan.ToString("g")); // "d.hh:mm:ss"
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(expectedTimeSpan, config.MyTimeSpan);
    }

    [Fact]
    public void TimeSpan_Missing_ReturnsDefault()
    {
        Environment.SetEnvironmentVariable("TEST_TIMESPAN", null);
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(default(TimeSpan), config.MyTimeSpan);
    }

    [Fact]
    public void TimeSpan_Invalid_ReturnsDefault()
    {
        Environment.SetEnvironmentVariable("TEST_TIMESPAN", "not-a-timespan");
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(default(TimeSpan), config.MyTimeSpan);
    }

    [Fact]
    public void Guid_Valid()
    {
        var expectedGuid = Guid.NewGuid();
        Environment.SetEnvironmentVariable("TEST_GUID", expectedGuid.ToString());
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(expectedGuid, config.MyGuid);
    }

    [Fact]
    public void Guid_Missing_ReturnsDefault()
    {
        Environment.SetEnvironmentVariable("TEST_GUID", null);
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(default(Guid), config.MyGuid);
    }

    [Fact]
    public void Guid_Invalid_ReturnsDefault()
    {
        Environment.SetEnvironmentVariable("TEST_GUID", "not-a-guid");
        var config = EnvConfig.Load<StandardTypesConfig>();
        Assert.Equal(default(Guid), config.MyGuid);
    }
}
