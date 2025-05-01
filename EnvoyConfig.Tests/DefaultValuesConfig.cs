using EnvoyConfig.Abstractions;
using EnvoyConfig.Attributes;

namespace EnvoyConfig.Tests
{
    public partial class DefaultValuesConfig : IEnvInitializable
    {
        [Env(Key = "TEST_STRING")]
        public string StringValue { get; set; } = "default";

        [Env(Key = "TEST_INT")]
        public int IntValue { get; set; } = 123;

        [Env(Key = "TEST_BOOL")]
        public bool BoolValue { get; set; } = false;
    }
}
