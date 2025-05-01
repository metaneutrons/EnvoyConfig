using System;
using EnvoyConfig;
using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

public class DirectKey
{
    public class DirectKeyConfig
    {
        [Env(Key = "TEST_INT")]
        public int IntValue { get; set; }

        [Env(Key = "TEST_STRING")]
        public string? StringValue { get; set; }

        [Env(Key = "TEST_BOOL")]
        public bool BoolValue { get; set; }

        [Env(Key = "TEST_ENUM")]
        public TestEnum EnumValue { get; set; }

        [Env(Key = "TEST_DEFAULT", Default = "default")]
        public string? Defaulted { get; set; }
    }

    public enum TestEnum
    {
        Foo,
        Bar,
        Baz,
    }

    [Fact]
    public void Loads_DirectKey_And_Default()
    {
        Environment.SetEnvironmentVariable("TEST_INT", "42");
        Environment.SetEnvironmentVariable("TEST_STRING", "hello");
        Environment.SetEnvironmentVariable("TEST_BOOL", "true");
        Environment.SetEnvironmentVariable("TEST_ENUM", "Bar");
        Environment.SetEnvironmentVariable("TEST_DEFAULT", null);
        var config = EnvConfig.Load<DirectKeyConfig>();
        Assert.Equal(42, config.IntValue);
        Assert.Equal("hello", config.StringValue);
        Assert.True(config.BoolValue);
        Assert.Equal(TestEnum.Bar, config.EnumValue);
        Assert.Equal("default", config.Defaulted);
    }
}
