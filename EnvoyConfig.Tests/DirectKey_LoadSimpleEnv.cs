using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class DirectKey_LoadSimpleEnv
    {
        [Fact]
        public void DirectKey_LoadSimpleEnv_Test()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TEST_SIMPLE", "42");

            // Act
            var config = EnvConfig.Load<TestConfig>(); // Central static API

            // Assert
            Assert.Equal(42, config.SimpleInt);
        }
    }
}
