using EnvoyConfig.Attributes;
using Xunit;

namespace EnvoyConfig.Tests;

public class Concurrency
{
    public class ConcurrencyConfig
    {
        [Env(Key = "CONCUR_INT")]
        public int IntValue { get; set; }
    }

    [Fact]
    public async Task Load_Is_ThreadSafe()
    {
        Environment.SetEnvironmentVariable("CONCUR_INT", "123");
        var tasks = new Task[10];
        for (var i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var config = EnvConfig.Load<ConcurrencyConfig>();
                Assert.Equal(123, config.IntValue);
            });
        }
        await Task.WhenAll(tasks);
    }
}
