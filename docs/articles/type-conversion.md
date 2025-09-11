# Type Conversion

EnvoyConfig attempts to convert the string values from environment variables to the target property types. Supported conversions typically include:

* `string` (no conversion needed)
* `int`, `long`, `short`, `byte`
* `uint`, `ulong`, `ushort`, `sbyte`
* `float`, `double`, `decimal`
* `bool` (case-insensitive "true", "1" usually map to `true`; others to `false`)
* `char` (first character of the string)
* `DateTime`, `DateTimeOffset`, `TimeSpan` (using standard parsing rules)
* `Uri`
* `Guid`
* `Enum` types (case-insensitive string matching to enum value names, with enhanced support via `[EnvValue]` attribute)
* `List<T>` (where T is one of the above simple types, for the `List` strategy)
* `Dictionary<string, string>` (for the `KeyValue` strategy)

**Error Handling:**
By default, if a conversion fails (e.g., trying to parse "abc" as an `int`), EnvoyConfig logs an error (if a logger is provided) and the property is set to its default C# value (e.g., `0` for `int`, `null` for reference types).

You can change this behavior by setting the static property `EnvConfig.ThrowOnConversionError = true;`. If set to `true`, `EnvConfig.Load<T>()` will throw an `InvalidOperationException` when a type conversion error occurs. This allows for stricter error handling if desired. See the "Validation & Error Handling" article for more details on this property.

Ensure you have appropriate error handling (like a `try-catch` block around `EnvConfig.Load<T>()`) if you enable `ThrowOnConversionError` or if you need to handle other potential loading issues.

## Enhanced Enum Support

EnvoyConfig provides enhanced enum support through the `[EnvValue]` attribute, allowing you to map custom string values to enum members. This is particularly useful when you want to accept user-friendly or alternative string representations for enum values.

### Basic Enum Usage

Without any attributes, enums work with standard case-insensitive name matching:

```csharp
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public class AppConfig
{
    [Env("LOG_LEVEL")]
    public LogLevel Level { get; set; }
}

// Environment: LOG_LEVEL=debug (case-insensitive)
// Result: LogLevel.Debug
```

### Enhanced Enum with EnvoyConfigValue Attribute

Use the `[EnvoyConfigValue]` attribute to define custom string mappings:

```csharp
public enum TranscodingFormat
{
    [EnvoyConfigValue("off", "disabled", "none")]
    Disabled,
    
    [EnvoyConfigValue("mp3")]
    Mp3,
    
    [EnvoyConfigValue("ogg", "vorbis")]
    Ogg,
    
    Flac // No attribute - uses standard enum parsing
}

public class AudioConfig
{
    [EnvoyConfig("TRANSCODING_FORMAT")]
    public TranscodingFormat Format { get; set; }
}
```

**Supported Environment Values:**
```bash
# Any of these map to TranscodingFormat.Disabled
TRANSCODING_FORMAT=off
TRANSCODING_FORMAT=disabled  
TRANSCODING_FORMAT=none

# This maps to TranscodingFormat.Mp3
TRANSCODING_FORMAT=mp3

# Either of these map to TranscodingFormat.Ogg
TRANSCODING_FORMAT=ogg
TRANSCODING_FORMAT=vorbis

# Standard enum parsing still works
TRANSCODING_FORMAT=Flac
```

### Serialization Behavior

When saving configurations using `EnvConfig.Save()`, the first value from the `[EnvoyConfigValue]` attribute is used:

```csharp
var config = new AudioConfig { Format = TranscodingFormat.Disabled };
EnvConfig.Save(config, "config.env");
// Output: TRANSCODING_FORMAT=off
```

### Fallback Behavior

The enum parsing follows this priority order:
1. **EnvoyConfigValue attribute mapping** (case-insensitive)
2. **Standard enum name parsing** (case-insensitive)
3. **Conversion error** (logged or thrown based on `ThrowOnConversionError` setting)

## Custom Type Converters

EnvoyConfig provides an extensibility point for handling types that are not natively supported or for scenarios where you require custom parsing logic for specific types. This is achieved by implementing the `ITypeConverter` interface and registering your converter with the `TypeConverterRegistry`.

### 1. Implement `ITypeConverter`

The `EnvoyConfig.Conversion.ITypeConverter` interface has one method:

```csharp
object? Convert(string? value, Type targetType, IEnvLogSink? logger);
```

- `value`: The string value from the environment variable.
- `targetType`: The `Type` of the property that needs to be set. Your converter should typically check if this matches the type it's designed to handle.
- `logger`: An optional `IEnvLogSink` instance that you can use to log any warnings or errors during the conversion process.

**Example:** Let's say you have a `Point` class and want to parse it from a "x,y" string format.

```csharp
// Your custom type
public class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public override string ToString() => $"({X},{Y})";
}

// Your custom converter
public class PointConverter : ITypeConverter
{
    public object? Convert(string? value, Type targetType, IEnvLogSink? logger)
    {
        if (targetType != typeof(Point))
        {
            // Should not happen if registered correctly, but good practice
            logger?.Log(EnvLogLevel.Error, "PointConverter was incorrectly invoked for a non-Point type.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            logger?.Log(EnvLogLevel.Info, "PointConverter received null or empty input.");
            return null; // Or return new Point(0,0); if that's a desired default
        }

        var parts = value.Split(',');
        if (parts.Length == 2 &&
            int.TryParse(parts[0].Trim(), out int x) &&
            int.TryParse(parts[1].Trim(), out int y))
        {
            return new Point { X = x, Y = y };
        }

        logger?.Log(EnvLogLevel.Error, $"PointConverter: Could not parse '{value}' into a Point object. Expected format 'x,y'.");
        return null; // Or throw, or return a specific default Point instance
    }
}
```

### 2. Register Your Converter

Before you call `EnvConfig.Load<T>()`, you need to register an instance of your custom converter with the `TypeConverterRegistry`:

```csharp
using EnvoyConfig.Conversion;

// ...
// Typically during application startup
TypeConverterRegistry.RegisterConverter(typeof(Point), new PointConverter());
// ...

// Now, when EnvConfig.Load<T>() encounters a property of type Point,
// it will use your PointConverter.
public class MyConfig
{
    [EnvoyConfig("APP_START_POINT")]
    public Point StartPoint { get; set; }
}

// ...
// var config = EnvConfig.Load<MyConfig>();
// Console.WriteLine(config.StartPoint); // Example: (10,20) if APP_START_POINT=10,20
```

By using custom type converters, you can extend EnvoyConfig to support virtually any data type that can be represented as a string in an environment variable.
