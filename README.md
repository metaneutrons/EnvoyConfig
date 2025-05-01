# EnvoyConfig

This repository contains:

- **EnvoyConfig.Abstractions**: shared attributes and interfaces for source generation.
- **EnvoyConfig.Generator**: Roslyn source generator producing extension methods based on environment attributes.
- **EnvoyConfig.Sample**: example project demonstrating generator output and usage.
- **EnvoyConfig.Adapters.***: logging adapters (Microsoft, NLog, Serilog).

> **⚠️ Reserved Property Name: `Environment`**
>
> Do **not** use `Environment` as a property name in your configuration classes. This name conflicts with the built-in `System.Environment` class and will cause ambiguous references or generator errors. Use a different name such as `Env` or `AppEnvironment`.
>
> If you use `Environment`, the EnvoyConfig generator will emit a warning and ignore the property.

See individual project README files for more details.
