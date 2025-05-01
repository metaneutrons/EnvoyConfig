using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class DirectKey_UseDefaultStringBoolWhenMissing
    {
        [Fact]
        public void DirectKey_UseDefaultStringBoolWhenMissing_Test()
        {
            Environment.SetEnvironmentVariable("TEST_STRING", null);
            Environment.SetEnvironmentVariable("TEST_BOOL", null);
            var config = EnvConfig.Load<DefaultValuesConfig>();
            Assert.Equal("default", config.StringValue);
            Assert.False(config.BoolValue);
        }
    }
}
