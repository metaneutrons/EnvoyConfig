<p align="center">
  <img src="assets/logo.svg" alt="EnvoyConfig Logo" width="64"/>
</p>

# 🚀 EnvoyConfig

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/metaneutrons/EnvoyConfig/actions)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)
[![NuGet](https://img.shields.io/nuget/v/EnvoyConfig)](https://www.nuget.org/packages/EnvoyConfig)
[![License: GPL-3.0-or-later](https://img.shields.io/badge/license-GPL--3.0--or--later-blue.svg)](LICENSE)
[![GitHub Copilot](https://img.shields.io/badge/GitHub-Copilot-blue?logo=github)](https://github.com/features/copilot)
[![Built with Claude](https://img.shields.io/badge/Built_with-Claude-8A2BE2)](https://claude.ai)
[![Status: Pre-Release](https://img.shields.io/badge/Status-Beta-yellow)](https://github.com/metaneutrons/EnvoyConfig/releases)

**EnvoyConfig** is a modern C# library for .NET 8+ that loads strongly-typed configuration objects from environment variables at runtime using reflection. It supports advanced attribute-based mapping, type safety, nested objects, lists, dictionaries, and flexible prefixing—all with minimal boilerplate.

## ✨ Features

- Attribute-based configuration: `[Env]` attribute for mapping properties to env vars
- Supports primitives, enums, nullable types, lists, arrays, dictionaries, and nested objects
- Multiple initialization modes: direct key, comma-separated/numbered lists, dictionary/map, nested prefix
- Global/static prefix support
- Logging integration with adapters (Microsoft, Serilog, NLog)
- Thread-safe, high performance (caching)
- Zero external dependencies in core

## 📚 Documentation

See here for [Documentation](https://metaneutrons.github.io/EnvoyConfig).

## 📦 Installation

```bash
dotnet add package EnvoyConfig
```

_Optional:_

```bash
dotnet add package EnvoyConfig.Adapters.Microsoft  # For Microsoft.Extensions.Logging
```

## 🚀 Quick Start

```csharp
public class ApiKeyConfig
{
    [Env(Key = "PRIMARY")]         // Maps to: MYAPP_API_PRIMARY
    public string Primary { get; set; } = "";

    [Env(Key = "SECONDARY")]       // Maps to: MYAPP_API_SECONDARY
    public string Secondary { get; set; } = "";

    [Env(Key = "ENABLED", Default = true)]  // Maps to: MYAPP_API_ENABLED
    public bool Enabled { get; set; }
}

public class MyConfig
{
    [Env(Key = "PORT", Default = 8080)]    // Maps to: MYAPP_PORT
    public int Port { get; set; }

    [Env(Key = "FEATURES", IsList = true)] // Maps to: MYAPP_FEATURES (comma-separated)
    public List<string> Features { get; set; } = new();

    [Env(NestedPrefix = "API_")]           // Maps to: MYAPP_API_* (nested object)
    public ApiKeyConfig ApiKeys { get; set; } = new();

    [Env(ListPrefix = "SERVER_")]          // Maps to: MYAPP_SERVER_1, MYAPP_SERVER_2, etc.
    public List<string> Servers { get; set; } = new();
}

// Set global prefix for all environment variables
EnvConfig.GlobalPrefix = "MYAPP_";

// Load configuration from environment variables
var config = EnvConfig.Load<MyConfig>();

// Save current configuration to .env file
EnvConfig.Save(config, "current-config.env");

// Save defaults template to .env file
EnvConfig.SaveDefaults<MyConfig>("template.env");
```

**Example Environment Variables:**

```bash
MYAPP_PORT=3000
MYAPP_FEATURES=auth,logging,metrics
MYAPP_API_PRIMARY=key123abc
MYAPP_API_SECONDARY=key456def
MYAPP_API_ENABLED=true
MYAPP_SERVER_1=api.example.com
MYAPP_SERVER_2=backup.example.com
```

## 🔧 Advanced Usage & Features

- **Prefix Handling:** Set `EnvConfig.GlobalPrefix` to prepend to all lookups.
- **Attribute Modes:**
  - `[Env(Key = "FOO")]` (direct key)
  - `[Env(Key = "BAR", IsList = true)]` (comma-separated list)
  - `[Env(ListPrefix = "ITEM_")]` (numbered list: ITEM_1, ITEM_2, ...)
  - `[Env(MapPrefix = "MAP_")]` (dictionary: MAP_key1=val1, MAP_key2=val2)
  - `[Env(NestedPrefix = "DB_")]` (nested object)
- **Supported Types:** Primitives (string, int, bool, double, etc.), `DateTime`, `TimeSpan`, `Guid`, enums, nullable types, List<T>, T[], Dictionary<TKey,TValue>. Custom types can be supported via `ITypeConverter`.
- **Logging:** Pass a custom logger (`IEnvLogSink`) or use an adapter for your framework.
- **Custom Type Converters:** Implement `ITypeConverter` and register with `TypeConverterRegistry.RegisterConverter()` to handle custom string-to-type conversions.
- **Error Handling:** Configure `EnvConfig.ThrowOnConversionError` (default `false`) to control behavior when type conversion fails. If `true`, exceptions are thrown; otherwise, errors are logged and default values are used.

## 🔧 Advanced Usage & Features (Continued)

### Custom Type Converters

EnvoyConfig allows you to define and register custom converters for types that are not natively supported or when you need specific parsing logic.

1. **Implement `ITypeConverter`:**
    Create a class that implements the `EnvoyConfig.Conversion.ITypeConverter` interface. The core method is `object? Convert(string? value, Type targetType, IEnvLogSink? logger)`.

    ```csharp
    // Example: A simple Point class and its converter
    public class Point { public int X { get; set; } public int Y { get; set; } }

    public class PointConverter : ITypeConverter
    {
        public object? Convert(string? value, Type targetType, IEnvLogSink? logger)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var parts = value.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
            {
                return new Point { X = x, Y = y };
            }
            logger?.Log(EnvLogLevel.Error, $"Cannot convert '{value}' to Point.");
            return null;
        }
    }
    ```

2. **Register the Converter:**
    Before loading your configuration, register an instance of your converter:

    ```csharp
    using EnvoyConfig.Conversion;
    // ...
    TypeConverterRegistry.RegisterConverter(typeof(Point), new PointConverter());
    // ...
    // var config = EnvConfig.Load<YourConfigWithAPointProperty>();
    ```

### Error Handling

Control how EnvoyConfig behaves when a type conversion from a string environment variable to a target property type fails:

- **`EnvConfig.ThrowOnConversionError` (static property):**
  - `false` (Default): If a conversion fails (e.g., "abc" cannot be parsed as an `int`), EnvoyConfig logs an error (if a logger is provided) and the property is set to its default C# value (e.g., `0` for `int`, `null` for reference types).
  - `true`: If a conversion fails, EnvoyConfig will throw an `InvalidOperationException` detailing the conversion issue. This allows for immediate and strict error handling.

    ```csharp
    // Example:
    EnvConfig.ThrowOnConversionError = true; // Optional: for stricter error handling
    try
    {
        // var config = EnvConfig.Load<MyConfig>();
    }
    catch (InvalidOperationException ex)
    {
        // Handle conversion errors
    }
    ```

## 🛠️ Troubleshooting / FAQ

- **Type conversion errors:** Check environment variable values and target property types. If `EnvConfig.ThrowOnConversionError` is `false` (default), check logs for details and verify if the property has its default C# value. If `true`, an exception will be thrown.
- **Missing env vars:** Use `Default` attribute property or handle `null`/default values in your application logic.
- **Prefix confusion:** Ensure `GlobalPrefix` and attribute keys/prefixes are set as intended.
- **Logging:** Implement `IEnvLogSink` or use provided adapters for structured logs, especially to diagnose conversion issues when `ThrowOnConversionError` is `false`.

## 🤝 Contributing

Contributions are welcome! Please open issues or PRs for bugs, features, or questions.

## 📜 License

GPL-3.0-or-later. See [LICENSE](LICENSE).
