# EnvoyConfig.Adapters.NLog

Adapter for integrating EnvoyConfig logging (`IEnvLogSink`) with NLog.

## Usage

```csharp
using EnvoyConfig.Adapters.NLog;
using NLog;

Logger logger = LogManager.GetCurrentClassLogger();
var adapter = new NLogLoggerAdapter(logger);
```

Reference this project and pass your NLog `Logger` instance to the adapter.
