using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class DirectKey_HandleInvalidBoolGracefully
    {
        [Fact]
        public void DirectKey_HandleInvalidBoolGracefully_Test()
        {
            Environment.SetEnvironmentVariable("TEST_BOOL", "notabool");
            var config = EnvConfig.Load<DefaultValuesConfig>();
            Assert.False(config.BoolValue); // Should fall back to default
        }
    }
}
