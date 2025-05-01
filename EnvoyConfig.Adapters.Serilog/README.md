# EnvoyConfig.Adapters.Serilog

Adapter for integrating EnvoyConfig logging (`IEnvLogSink`) with Serilog.

## Usage

```csharp
using EnvoyConfig.Adapters.Serilog;
using Serilog;

ILogger logger = ...;
var adapter = new SerilogLoggerAdapter(logger);
```

Reference this project and pass your Serilog `ILogger` instance to the adapter.
