using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

public class EdgeCases
{
    public class EdgeConfig
    {
        [Env(Key = "EDGE_INT")]
        public int IntValue { get; set; }

        [Env(Key = "EDGE_BOOL")]
        public bool BoolValue { get; set; }
    }

    [Fact]
    public void Handles_Missing_And_Invalid_Values()
    {
        Environment.SetEnvironmentVariable("EDGE_INT", null);
        Environment.SetEnvironmentVariable("EDGE_BOOL", "notabool");
        var config = EnvConfig.Load<EdgeConfig>();
        Assert.Equal(0, config.IntValue); // default(int)
        Assert.False(config.BoolValue); // default(bool)
    }
}
