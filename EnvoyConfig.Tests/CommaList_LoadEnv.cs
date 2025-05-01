using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class CommaList_LoadEnv
    {
        [Fact]
        public void CommaList_LoadEnv_Test()
        {
            Environment.SetEnvironmentVariable("NUMBERS", "1,2,3");
            var config = EnvConfig.Load<CommaListConfig>();
            Assert.Equal(new[] { 1, 2, 3 }, config.Numbers);
        }
    }
}
