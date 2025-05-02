# Logging Integration

EnvoyConfig provides a simple logging abstraction to allow you to capture internal messages generated during the configuration loading process. This helps diagnose issues like missing variables, used defaults, or parsing errors.

## Logging Interface (`IEnvLogSink`)

The core library defines the `EnvoyConfig.Logging.IEnvLogSink` interface:

```csharp
namespace EnvoyConfig.Logging;

using System;

public interface IEnvLogSink
{
    void Log(EnvLogLevel level, string message, Exception? exception = null);
}
```

And the corresponding log levels:

```csharp
namespace EnvoyConfig.Logging;

public enum EnvLogLevel
{
    Off = 0, Error = 1, Warning = 2, Info = 3, Debug = 4
}
```

## Configuring Logging

Before calling `EnvConfig.Load<T>()`, you can assign an implementation of `IEnvLogSink` to the static `EnvConfig.Logger` property. You can also control the minimum level of messages to be processed using `EnvConfig.LogLevel` (defaults to `Error`).

```csharp
using EnvoyConfig;
using EnvoyConfig.Logging;

// Assign your logger implementation (see Adapters below)
EnvConfig.Logger = myLogSinkImplementation;

// Set the minimum level (optional, default is Error)
EnvConfig.LogLevel = EnvLogLevel.Debug;

// Now load configuration
var config = EnvConfig.Load<MyConfig>();
```

## Logging Behavior

* **Debug:** Logs detailed steps like starting the load process, effective prefix used.
* **Info:** Logs successful completion of loading for a type.
* **Warning:** Logs when a default value is used because an environment variable was missing.
* **Error:** Logs when a required variable (non-nullable value type without a `Default`) is missing, or when parsing a value (from environment or `Default`) fails (e.g., `FormatException`). In case of a parse error, the property retains its default C# value.

EnvoyConfig aims *not* to throw exceptions for common configuration issues but logs them instead. Reflection exceptions might still occur for fundamental issues.

## Logging Adapters

To easily integrate with common logging frameworks, EnvoyConfig provides adapter packages:

* **`EnvoyConfig.Adapters.Microsoft`**: for `Microsoft.Extensions.Logging`.
* **`EnvoyConfig.Adapters.Serilog`**: for Serilog.
* **`EnvoyConfig.Adapters.NLog`**: for NLog.

**Installation:**

```bash
dotnet add package EnvoyConfig.Adapters.Microsoft
dotnet add package EnvoyConfig.Adapters.Serilog
dotnet add package EnvoyConfig.Adapters.NLog
```

**Usage Example (Microsoft.Extensions.Logging):**

```csharp
using EnvoyConfig;
using EnvoyConfig.Adapters.Microsoft;
using Microsoft.Extensions.Logging;

// Assuming you have an ILoggerFactory or ILogger instance
ILoggerFactory loggerFactory = ...;
ILogger envoyLogger = loggerFactory.CreateLogger("EnvoyConfig");

// Create the adapter and assign it
EnvConfig.Logger = new MicrosoftLoggerAdapter(envoyLogger);
EnvConfig.LogLevel = EnvLogLevel.Debug; // Optional

// Load config
var config = EnvConfig.Load<YourConfig>();
```

**Usage Example (Serilog):**

```csharp
using EnvoyConfig;
using EnvoyConfig.Adapters.Serilog;
using Serilog;

// Assuming you have a Serilog ILogger instance
ILogger serilogLogger = ...;

// Create the adapter and assign it
EnvConfig.Logger = new SerilogLoggerAdapter(serilogLogger);
EnvConfig.LogLevel = EnvLogLevel.Debug; // Optional

// Load config
var config = EnvConfig.Load<YourConfig>();
```

**Usage Example (NLog):**

```csharp
using EnvoyConfig;
using EnvoyConfig.Adapters.NLog;
using NLog;

// Assuming you have an NLog Logger instance
Logger nlogLogger = LogManager.GetCurrentClassLogger();

// Create the adapter and assign it
EnvConfig.Logger = new NLogLoggerAdapter(nlogLogger);
EnvConfig.LogLevel = EnvLogLevel.Debug; // Optional

// Load config
var config = EnvConfig.Load<YourConfig>();
```
