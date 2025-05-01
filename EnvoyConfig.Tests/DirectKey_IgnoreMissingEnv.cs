using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class DirectKey_IgnoreMissingEnv
    {
        [Fact]
        public void DirectKey_IgnoreMissingEnv_Test()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TEST_MISSING_ISOLATED", null);

            // Act
            var config = EnvConfig.Load<TestConfig>(); // Central static API

            // Assert
            Assert.Equal(0, config.SimpleIntIsolated); // default value
        }
    }
}
