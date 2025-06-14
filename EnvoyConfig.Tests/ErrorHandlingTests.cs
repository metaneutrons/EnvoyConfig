using System;
using System.Collections.Generic;
using EnvoyConfig.Attributes;
using EnvoyConfig.Conversion;
using EnvoyConfig.Logging;
using Xunit;

// Assuming CustomPoint and ConfigWithCustomType are accessible from EnvoyConfig.Tests namespace
// If TestLogSink is in CustomConverterTests.cs, it should also be accessible.

namespace EnvoyConfig.Tests
{
    public class ErrorConfig
    {
        [Env("BAD_INT")]
        public int MyBadInt { get; set; }
    }

    public class FaultyCustomPointConverter : ITypeConverter
    {
        public object? Convert(string? value, Type targetType, IEnvLogSink? logger)
        {
            logger?.Log(EnvLogLevel.Error, "FaultyCustomPointConverter: Intentionally returning null.");
            return null;
        }
    }

    public class ConversionErrorHandlingTests : IDisposable
    {
        private readonly bool _originalThrowState;
        private readonly TestLogSink _testLogSink;

        public ConversionErrorHandlingTests()
        {
            _originalThrowState = EnvConfig.ThrowOnConversionError;
            _testLogSink = new TestLogSink(); // Assumes TestLogSink is accessible

            // Ensure a known state for CustomPoint converter for relevant tests.
            // Registering a null converter here to avoid interference from other test files
            // if CustomPoint was registered globally.
            TypeConverterRegistry.RegisterConverter(typeof(CustomPoint), new NullCustomPointConverter());
        }

        [Fact]
        public void ThrowOnConversionError_True_StandardType_ThrowsInvalidOperationException()
        {
            EnvConfig.ThrowOnConversionError = true;
            Environment.SetEnvironmentVariable("BAD_INT", "not-an-int");

            try
            {
                var ex = Assert.Throws<InvalidOperationException>(() => EnvConfig.Load<ErrorConfig>());
                Assert.Contains("Failed to convert 'BAD_INT' value 'not-an-int' to int", ex.Message);
            }
            finally
            {
                Environment.SetEnvironmentVariable("BAD_INT", null);
            }
        }

        [Fact]
        public void ThrowOnConversionError_True_CustomConverterReturnsNull_DoesNotThrowFromReflectionHelper()
        {
            EnvConfig.ThrowOnConversionError = true;
            TypeConverterRegistry.RegisterConverter(typeof(CustomPoint), new FaultyCustomPointConverter());
            Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", "trigger-error");

            try
            {
                // No exception expected here from EnvConfig.Load itself
                var config = EnvConfig.Load<ConfigWithCustomType>(_testLogSink);
                Assert.Null(config.MyCustomPoint);
                Assert.Contains(_testLogSink.Messages, msg => msg.Level == EnvLogLevel.Error && msg.Message.Contains("FaultyCustomPointConverter: Intentionally returning null."));
            }
            finally
            {
                Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", null);
                // Reset to a neutral converter for CustomPoint to avoid interference
                TypeConverterRegistry.RegisterConverter(typeof(CustomPoint), new NullCustomPointConverter());
            }
        }

        [Fact]
        public void ThrowOnConversionError_False_StandardType_LogsAndDefaults()
        {
            EnvConfig.ThrowOnConversionError = false; // Should be default, but explicit
            Environment.SetEnvironmentVariable("BAD_INT", "not-an-int");

            try
            {
                var config = EnvConfig.Load<ErrorConfig>(_testLogSink);
                Assert.Equal(0, config.MyBadInt); // Default for int
                Assert.Contains(_testLogSink.Messages, msg => msg.Level == EnvLogLevel.Error && msg.Message.Contains("Failed to convert 'BAD_INT' value 'not-an-int' to int"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("BAD_INT", null);
            }
        }

        public void Dispose()
        {
            EnvConfig.ThrowOnConversionError = _originalThrowState;
            // Clean up environment variables if any were set globally for a class fixture (not the case here per test)
            // Environment.SetEnvironmentVariable("BAD_INT", null);
            // Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", null);
        }
    }

    // If TestLogSink is not in a shared file, it needs to be defined here or made accessible.
    // For this exercise, we assume it's accessible (e.g. from CustomConverterTests.cs or a shared utils file).
    // public class TestLogSink : IEnvLogSink { ... }
    // (Copied from CustomConverterTests.cs for completeness if needed, but ideally defined once)
    // To avoid redefinition error if this file is processed after CustomConverterTests.cs:
    // Ensure TestLogSink is in its own file or use a preprocessor directive.
    // For now, I will assume it's available and not redefine it here.
    //
    // Similarly, NullCustomPointConverter is also assumed to be available.
    // internal class NullCustomPointConverter : ITypeConverter { ... }
}
```
