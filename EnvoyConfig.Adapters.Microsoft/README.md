# EnvoyConfig.Adapters.Microsoft

Adapter for integrating EnvoyConfig logging (`IEnvLogSink`) with Microsoft.Extensions.Logging.

## Usage

```csharp
using EnvoyConfig.Adapters.Microsoft;
using Microsoft.Extensions.Logging;

ILogger logger = ...;
var adapter = new MicrosoftLoggerAdapter(logger);
```

Reference this project and pass your ILogger instance to the adapter.
