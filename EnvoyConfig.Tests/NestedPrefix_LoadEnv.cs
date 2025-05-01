using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class NestedPrefix_LoadEnv
    {
        [Fact]
        public void NestedPrefix_LoadEnv_Test()
        {
            Environment.SetEnvironmentVariable("PARENT_CHILD_VALUE", "5");
            var config = EnvConfig.Load<NestedConfig>();
            Assert.NotNull(config.Child);
            Assert.Equal(5, config.Child.ChildValue);
        }
    }
}
