using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnvoyConfig.Attributes;
using EnvoyConfig.Conversion;
using EnvoyConfig.Logging;
using Xunit;

namespace EnvoyConfig.Tests
{
    // 1. Define CustomPoint
    public class CustomPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        // Default constructor for Activator.CreateInstance in ReflectionHelper if no valid env var is found
        // and no custom converter is registered, or if the custom converter returns null.
        public CustomPoint()
        {
            X = 0;
            Y = 0;
        }

        public CustomPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj)
        {
            if (obj is CustomPoint other)
            {
                return X == other.X && Y == other.Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString() => $"{X},{Y}";
    }

    // 2. Define CustomPointConverter
    public class CustomPointConverter : ITypeConverter
    {
        public object? Convert(string? value, Type targetType, IEnvLogSink? logger)
        {
            if (targetType != typeof(CustomPoint))
            {
                // This check is good practice, though ReflectionHelper currently calls the converter only if the type matches.
                logger?.Log(
                    EnvLogLevel.Error,
                    $"CustomPointConverter: This converter only supports CustomPoint, not {targetType.Name}."
                );
                return null;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                logger?.Log(
                    EnvLogLevel.Warning,
                    $"CustomPointConverter: Input string for CustomPoint is null or whitespace. Returning null."
                );
                return null;
            }

            var parts = value.Split(',');
            if (parts.Length != 2)
            {
                logger?.Log(
                    EnvLogLevel.Error,
                    $"CustomPointConverter: Invalid input string format for CustomPoint. Expected 'X,Y', got '{value}'. Returning null."
                );
                return null;
            }

            if (
                int.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out int x)
                && int.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out int y)
            )
            {
                return new CustomPoint(x, y);
            }
            else
            {
                logger?.Log(
                    EnvLogLevel.Error,
                    $"CustomPointConverter: Failed to parse X or Y integer values from '{value}' for CustomPoint. Returning null."
                );
                return null;
            }
        }
    }

    // 3. Define ConfigWithCustomType
    public class ConfigWithCustomType
    {
        [Env("CUSTOM_POINT_VAR")]
        public CustomPoint? MyCustomPoint { get; set; }
    }

    // Dummy converter for trying to simulate "unregistering" CustomPointConverter
    internal class NullCustomPointConverter : ITypeConverter
    {
        public object? Convert(string? value, Type targetType, IEnvLogSink? logger) => null;
    }

    // 4. Test Class
    public class CustomConverterFeatureTests : IDisposable
    {
        private readonly TestLogSink _testLogger;

        public CustomConverterFeatureTests()
        {
            EnvConfig.ThrowOnConversionError = false;
            _testLogger = new TestLogSink();

            // Attempt to ensure a "clean slate" for CustomPoint converters before each test.
            // This registers a converter that will always return null for CustomPoint,
            // effectively making it seem like no "useful" converter is registered.
            // Tests that need the real CustomPointConverter must register it themselves overwritting this.
            TypeConverterRegistry.RegisterConverter(typeof(CustomPoint), new NullCustomPointConverter());
        }

        [Fact]
        public void CustomConverter_Registered_ConvertsSuccessfully()
        {
            // Arrange
            TypeConverterRegistry.RegisterConverter(typeof(CustomPoint), new CustomPointConverter());
            Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", "10,20");

            // Act
            var config = EnvConfig.Load<ConfigWithCustomType>(_testLogger);

            // Assert
            Assert.NotNull(config.MyCustomPoint);
            Assert.Equal(10, config.MyCustomPoint.X);
            Assert.Equal(20, config.MyCustomPoint.Y);
            // CustomPointConverter does not log on success
            Assert.Empty(_testLogger.Messages);
        }

        [Fact]
        public void CustomConverter_Registered_HandlesInvalidFormatInputGracefully()
        {
            // Arrange
            TypeConverterRegistry.RegisterConverter(typeof(CustomPoint), new CustomPointConverter());
            Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", "foo,bar,baz"); // Invalid format

            // Act
            var config = EnvConfig.Load<ConfigWithCustomType>(_testLogger);

            // Assert
            Assert.Null(config.MyCustomPoint); // Converter returns null on error
            Assert.Contains(
                _testLogger.Messages,
                msg =>
                    msg.Level == EnvLogLevel.Error
                    && msg.Message.StartsWith("CustomPointConverter: Invalid input string format")
            );
        }

        [Fact]
        public void CustomConverter_Registered_HandlesInvalidIntegerPartsGracefully()
        {
            // Arrange
            TypeConverterRegistry.RegisterConverter(typeof(CustomPoint), new CustomPointConverter());
            Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", "10,NaN");

            // Act
            var config = EnvConfig.Load<ConfigWithCustomType>(_testLogger);

            // Assert
            Assert.Null(config.MyCustomPoint); // Converter returns null on error
            Assert.Contains(
                _testLogger.Messages,
                msg =>
                    msg.Level == EnvLogLevel.Error
                    && msg.Message.StartsWith("CustomPointConverter: Failed to parse X or Y integer values")
            );
        }

        [Fact]
        public void CustomConverter_Registered_HandlesEmptyInputGracefully()
        {
            // Arrange
            TypeConverterRegistry.RegisterConverter(typeof(CustomPoint), new CustomPointConverter());
            Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", "");

            // Act
            var config = EnvConfig.Load<ConfigWithCustomType>(_testLogger);

            // Assert
            Assert.Null(config.MyCustomPoint); // Converter returns null for empty/whitespace
            Assert.Contains(
                _testLogger.Messages,
                msg =>
                    msg.Level == EnvLogLevel.Warning
                    && msg.Message.StartsWith(
                        "CustomPointConverter: Input string for CustomPoint is null or whitespace"
                    )
            );
        }

        [Fact]
        public void CustomConverter_EffectivelyNotRegistered_PropertyRemainsNullAndNoConversionLog()
        {
            // Arrange
            // The constructor registers NullCustomPointConverter for CustomPoint.
            // This converter always returns null and does not log.
            Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", "10,20");

            // Act
            var config = EnvConfig.Load<ConfigWithCustomType>(_testLogger);

            // Assert
            // MyCustomPoint should be null because NullCustomPointConverter returns null.
            Assert.Null(config.MyCustomPoint);

            // NullCustomPointConverter doesn't log.
            // ReflectionHelper receives null from the custom converter. Since the value is null,
            // it doesn't attempt further conversion or log a failure for this value; it uses the null.
            Assert.Empty(_testLogger.Messages);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("CUSTOM_POINT_VAR", null);
            // This is where an UnregisterConverter would be ideal:
            // TypeConverterRegistry.UnregisterConverter(typeof(CustomPoint));
            // For now, the constructor tries to reset the state for CustomPoint for each test run.
        }
    }

    // TestLogSink implementation for testing purposes
    public class TestLogSink : IEnvLogSink
    {
        public readonly List<(EnvLogLevel Level, string Message)> Entries = new();
        public IEnumerable<(EnvLogLevel Level, string Message)> Messages => Entries;

        public void Log(EnvLogLevel level, string message) => Entries.Add((level, message));
    }
}
