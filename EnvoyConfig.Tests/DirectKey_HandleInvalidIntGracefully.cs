using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class DirectKey_HandleInvalidIntGracefully
    {
        [Fact]
        public void DirectKey_HandleInvalidIntGracefully_Test()
        {
            Environment.SetEnvironmentVariable("TEST_INT", "not_an_int");
            var config = EnvConfig.Load<DefaultValuesConfig>();
            Assert.Equal(123, config.IntValue); // Should fall back to default
        }
    }
}
