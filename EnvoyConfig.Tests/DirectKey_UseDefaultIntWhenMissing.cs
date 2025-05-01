using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class DirectKey_UseDefaultIntWhenMissing
    {
        [Fact]
        public void DirectKey_UseDefaultIntWhenMissing_Test()
        {
            Environment.SetEnvironmentVariable("TEST_INT", null);
            var config = EnvConfig.Load<DefaultValuesConfig>();
            Assert.Equal(123, config.IntValue);
        }
    }
}
