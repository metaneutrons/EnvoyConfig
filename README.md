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
public class MyConfig {
    [Env(Key = "PORT", Default = "8080")]  // for env var MYAPP_PORT
    public int Port { get; set; }

    [Env(Key = "FEATURES", IsList = true)] // for env var MYAPP_FEATURES
    public List<string> Features { get; set; } = new();
}

EnvConfig.GlobalPrefix = "MYAPP_";
var config = EnvConfig.Load<MyConfig>();
```

## 🔧 Advanced Usage & Features

- **Prefix Handling:** Set `EnvConfig.GlobalPrefix` to prepend to all lookups.
- **Attribute Modes:**
  - `[Env(Key = "FOO")]` (direct key)
  - `[Env(Key = "BAR", IsList = true)]` (comma-separated list)
  - `[Env(ListPrefix = "ITEM_")]` (numbered list: ITEM_1, ITEM_2, ...)
  - `[Env(MapPrefix = "MAP_")]` (dictionary: MAP_key1=val1, MAP_key2=val2)
  - `[Env(NestedPrefix = "DB_")]` (nested object)
- **Supported Types:** string, int, bool, double, enums, nullable types, List<T>, T[], Dictionary<TKey,TValue>
- **Logging:** Pass a custom logger (`IEnvLogSink`) or use an adapter for your framework.

## 📚 Documentation

See here for [Documentation](https://metaneutrons.github.io/EnvoyConfig).

## 🛠️ Troubleshooting / FAQ

- Type conversion errors: check env var values and types
- Missing env vars: use `Default` or handle nulls
- Prefix confusion: ensure GlobalPrefix and attribute keys are set as intended
- Logging: implement or use provided adapters for structured logs

## 🤝 Contributing

Contributions are welcome! Please open issues or PRs for bugs, features, or questions.

## 📜 License

GPL-3.0-or-later. See [LICENSE](LICENSE).
