using EnvoyConfig.Conversion;
using EnvoyConfig.Logging;
using Xunit;

namespace EnvoyConfig.Tests
{
    [CollectionDefinition("EnvoyConfig", DisableParallelization = true)]
    public class EnvoyConfigCollectionDefinition : ICollectionFixture<EnvoyConfigTestFixture> { }

    public class EnvoyConfigTestFixture : IDisposable
    {
        public EnvoyConfigTestFixture()
        {
            // Reset static state before tests
            ResetStaticState();
        }

        public void ResetStaticState()
        {
            // Clear converter registry
            TypeConverterRegistry.Clear();

            // Reset EnvConfig static properties to defaults
            EnvConfig.ThrowOnConversionError = false;
            EnvConfig.LogLevel = EnvLogLevel.Error;
            EnvConfig.Logger = null;
            EnvConfig.GlobalPrefix = string.Empty;
        }

        public void Dispose()
        {
            // Cleanup after tests
            ResetStaticState();
        }
    }
}
