using System;
using EnvoyConfig;
using Xunit;

namespace EnvoyConfig.Tests
{
    public class NumberedList_LoadEnv
    {
        [Fact]
        public void NumberedList_LoadEnv_Test()
        {
            Environment.SetEnvironmentVariable("SEQ_1", "first");
            Environment.SetEnvironmentVariable("SEQ_2", "second");
            Environment.SetEnvironmentVariable("SEQ_3", null);
            var config = EnvConfig.Load<NumberedListConfig>();
            Assert.Equal(new[] { "first", "second" }, config.Items);
        }
    }
}
