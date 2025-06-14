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
* `Enum` types (case-insensitive string matching to enum value names)
* `List<T>` (where T is one of the above simple types, for the `List` strategy)
* `Dictionary<string, string>` (for the `KeyValue` strategy)

**Error Handling:**
By default, if a conversion fails (e.g., trying to parse "abc" as an `int`), EnvoyConfig logs an error (if a logger is provided) and the property is set to its default C# value (e.g., `0` for `int`, `null` for reference types).

You can change this behavior by setting the static property `EnvConfig.ThrowOnConversionError = true;`. If set to `true`, `EnvConfig.Load<T>()` will throw an `InvalidOperationException` when a type conversion error occurs. This allows for stricter error handling if desired. See the "Validation & Error Handling" article for more details on this property.

Ensure you have appropriate error handling (like a `try-catch` block around `EnvConfig.Load<T>()`) if you enable `ThrowOnConversionError` or if you need to handle other potential loading issues.

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
    [Env("APP_START_POINT")]
    public Point StartPoint { get; set; }
}

// ...
// var config = EnvConfig.Load<MyConfig>();
// Console.WriteLine(config.StartPoint); // Example: (10,20) if APP_START_POINT=10,20
```

By using custom type converters, you can extend EnvoyConfig to support virtually any data type that can be represented as a string in an environment variable.
