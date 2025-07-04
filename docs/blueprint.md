# EnvoyConfig - Detailed Blueprint & Implementation Plan

**Version:** 5.0
**Date:** 2025-04-13
**Author:** Fabian Schmieder
**License:** LGPL-3.0-or-later

## 0. Goals & Scope

This document provides a complete blueprint for building the EnvoyConfig library from scratch using **Runtime Reflection**. EnvoyConfig simplifies application configuration by initializing C# objects from environment variables at runtime.

The primary goal is to create a robust, maintainable, and easy-to-use library that supports various configuration patterns through environment variables, leveraging runtime reflection and modern .NET features and best practices.

### Scope

- Core library for loading configuration via reflection.
- Attribute definitions (`EnvAttribute`) and logging interfaces (`IEnvLogSink`) within the core library.
- Logging adapters for common frameworks.
- Sample application demonstrating usage.
- Test suite for verifying functionality.
- Packaging for NuGet distribution.

## 1. Overview

EnvoyConfig is a modern C# library designed for .NET 8 applications, simplifying configuration by automatically initializing properties from environment variables at **runtime using Reflection**. It allows developers to define strongly-typed configuration classes and populate them seamlessly from the environment.

### 1.1. Core Features

- **Attribute-Based:** Uses a flexible `[Env]` attribute (defined within `EnvoyConfig`) to mark properties for initialization based on environment variable keys or prefixes.
- **Runtime Reflection:** Leverages .NET Reflection APIs (`System.Reflection`) to discover properties with `[Env]` attributes at runtime and dynamically read/parse environment variables to populate them.
- **Type Support:** Handles common primitives (string, int, bool, double, Guid, TimeSpan, DateTime, etc.), nullable value types, enums, nested complex objects (classes/structs), `List<T>`, `T[]` arrays, and `Dictionary<TKey, TValue>` (where T, TKey, TValue are supported primitives, enums, or strings).
- **Initialization Modes:**
  - **Direct Key:** `[Env(Key = "MY_VAR")]` maps a property to a single environment variable. Supports optional `Default` value. (Supported Types: Primitives, Enums, Nullables, String).
  - **Comma-Separated List/Array:** `[Env(Key = "MY_LIST_VAR", IsList = true, ListSeparator = ',')]` maps a `List<T>` or `T[]` property (where T is primitive/enum/string) to a single delimiter-separated environment variable. Supports optional `Default` value for the *entire* list string. (Supported Types: `List<T>`, `T[]`).
  - **Numbered List Prefix:** `[Env(ListPrefix = "MY_ITEM_")]` maps a `List<T>` or `T[]` property (where T is primitive/enum/string) to environment variables named sequentially (e.g., `MY_ITEM_1`, `MY_ITEM_2`, ...) starting with index 1. Stops at the first missing index or parse error. (Supported Types: `List<T>`, `T[]`).
  - **Dictionary Key-Value Prefix:** `[Env(MapPrefix = "MY_MAP_")]` maps a `Dictionary<TKey, TValue>` property (where TKey/TValue are primitive/enum/string) to environment variables named with the prefix (e.g., `MY_MAP_Key1=Value1`, `MY_MAP_Key2=Value2`). The part after the prefix is treated as the dictionary key. (Supported Types: `Dictionary<TKey, TValue>`).
  - **Nested Object/List Prefix:**
    - `[Env(NestedPrefix = "MQTT_ZONE_1_")]` applied to a complex object property (class/struct). Triggers recursive initialization of that object's properties using reflection, prepending the specified prefix to the `Key`/`ListPrefix`/`MapPrefix`/`NestedPrefix` values defined within the nested object's `[Env]` attributes. (Supported Types: Class, Struct).
    - `[Env(NestedListPrefix = "ZONE_", NestedListSuffix = "_MQTT_")]` applied to a `List<T>` property (where `T` is a class with `[Env]`-decorated properties). This pattern loads multiple objects from environment variables named like `ZONE_1_MQTT_*`, `ZONE_2_MQTT_*`, etc., and populates the list accordingly.

      **Example:**

      ```csharp
      public class ZoneMqttConfig
      {
          [Env(Key = "CONTROL_SET_TOPIC")]
          public string ControlSetTopic { get; set; }
          // ... other properties ...
      }

      public class ZonesConfig
      {
          [Env(NestedListPrefix = "ZONE_", NestedListSuffix = "_MQTT_")]
          public List<ZoneMqttConfig> ZonesMqtt { get; set; }
      }
      ```

      With environment variables:

      ```env
      ZONE_1_MQTT_CONTROL_SET_TOPIC=control/set
      ZONE_1_MQTT_TRACK_SET_TOPIC=track/set
      ZONE_2_MQTT_CONTROL_SET_TOPIC=control/set2
      ZONE_2_MQTT_TRACK_SET_TOPIC=track/set2
      ```

      The loader will create `ZonesMqtt[0]` and `ZonesMqtt[1]` with properties populated from the corresponding variables.

      See `EnvoyConfig.Tests/NestedList.cs` for a working test case.
- **Flexible Prefix Handling:** Facilitates organizing environment variables:
  - **Global Static Prefix:** `EnvConfig.GlobalPrefix` can be set once (e.g., "MYAPP\_") to be automatically prepended to all environment variable lookups.
  - **Per-Call Override Prefix:** The `EnvConfig.Load<T>(globalPrefixOverride: "...")` method allows specifying a prefix for a specific load operation, taking precedence over `EnvConfig.GlobalPrefix`.
- **Defaults & Null Handling:** Supports optional `Default` string values for `Key` and `CommaList/Array` modes. Handles missing environment variables gracefully (properties retain default C# values or become null/empty collections). Logs warnings/errors for missing required variables or failed parsing via `IEnvLogSink`.
- **Logging Integration:** Provides a simple logging abstraction (`IEnvLogSink` within `EnvoyConfig.Logging`) to allow consumers to integrate EnvoyConfig's internal operational logs (e.g., warnings about defaults, parse errors) into their application's logging framework. Adapter packages are provided.
- **Minimal Dependencies:** The core library (`EnvoyConfig`) has no external dependencies beyond the .NET 8 runtime. Consuming applications only need to reference the core library and optionally, logging adapters.
- **Performance & Caching:** Reflection metadata and attribute lookups are cached per type in a thread-safe store to optimize repeated `EnvConfig.Load<T>()` calls and ensure concurrent safety.

### 1.2. Technology Stack

- **.NET 8:** Target framework for all projects (Core, Adapters, Sample, Tests).
- **C# 12:** Utilizing modern language features where appropriate.
- **System.Reflection:** Core mechanism for discovering attributes and setting properties at runtime.
- **MSTest:** Chosen testing framework (`Microsoft.NET.Test.Sdk`, `MSTest.*` packages).
- **Moq:** Chosen mocking library for testing logging integration (`Moq` package).
- **GitVersion:** Assumed to be used externally for managing SemVer 2.0 versioning based on Git history.

## 2. Coding Conventions & Practices

Adherence to standard .NET coding conventions and modern best practices is expected.

### 2.1. Target Frameworks

- **All Projects:** `net8.0` (`EnvoyConfig`, `EnvoyConfig.Adapters.*`, `EnvoyConfig.Tests`, `EnvoyConfig.Sample`)

### 2.2. Language Features (C# 12)

- Utilize C# 12 features where they enhance clarity (e.g., file-scoped namespaces, collection expressions `[]`).
- Enable `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>` for all projects.

### 2.3. Naming & Style

- Follow standard .NET Naming Conventions (PascalCase for types/methods/properties, camelCase for locals/parameters).
- Use file-scoped namespaces (`namespace MyNamespace;`).
- Prefer `var` when the type is obvious.
- Utilize expression-bodied members for simple getters/methods.
- Adhere to formatting guidelines (use `dotnet format`).
- Add XML documentation comments (`/// <summary>...`) for all public types and members.

### 2.4. Error Handling (Logging vs Exceptions)

- **Core Library (`EnvConfig.Load<T>` & `ReflectionHelper`):**
  - Should *not* throw exceptions for common configuration issues like missing optional variables or using defaults. Log these at Debug/Warning via `IEnvLogSink`.
  - Should log an Error via `IEnvLogSink` if a required variable (non-nullable value type property without a `Default` attribute) is missing, or if a value (from environment or `Default` attribute) fails to parse.
  - In case of a parse error, the property should retain its default C# value (or `null`/empty collection).
  - *May* throw Reflection-related exceptions (`TargetInvocationException`, `MemberAccessException`, `ArgumentException` from `PropertyInfo.SetValue`, etc.) if fundamental reflection operations fail unexpectedly. These indicate potential programming errors or incompatible types.
  - Aim to catch `FormatException` during parsing and log errors via `IEnvLogSink`.
- **Adapters:** Should catch potential exceptions from the underlying logging framework during their `Log` call to prevent adapter failures from crashing the configuration process.

### 2.5. Performance & Thread-Safety

- Reflection scans (property/attribute discovery) are cached in a `ConcurrentDictionary<Type,...>` to minimize overhead on repeated loads.
- `EnvConfig.Load<T>()` and its caching layer are fully thread-safe and can be called concurrently.

### 2.6. Dependency Management

- Minimize external dependencies in the `EnvoyConfig` Core library (ideally none beyond the framework).
- Adapter libraries should only reference their specific logging framework and the `EnvoyConfig` core library.

## 3. Project Structure (Flat, Merged)

All projects reside directly under the main solution root directory.

### 3.1. ASCII Tree Diagram

```plaintext
EnvoyConfig/ # Root directory (Solution Location)
├── .gitignore
├── README.md
├── LICENSE
├── EnvoyConfig.sln # Solution file
│
├── EnvoyConfig/ # Core library project (net8.0)
│ ├── EnvoyConfig.csproj
│ ├── Attributes/
│ │ └── EnvAttribute.cs
│ ├── Logging/
│ │ ├── IEnvLogSink.cs
│ │ └── EnvLogLevel.cs
│ ├── EnvConfig.cs # Main public static class and Load<T> method
│ └── Internal/
│ └── ReflectionHelper.cs # Core reflection logic
│
├── EnvoyConfig.Adapters.Microsoft/ # Microsoft Adapter (net8.0)
│ ├── EnvoyConfig.Adapters.Microsoft.csproj # References EnvoyConfig
│ └── MicrosoftLoggerAdapter.cs
│
├── EnvoyConfig.Adapters.Serilog/ # Serilog Adapter (net8.0)
│ ├── EnvoyConfig.Adapters.Serilog.csproj # References EnvoyConfig
│ └── SerilogLoggerAdapter.cs
│
├── EnvoyConfig.Adapters.NLog/ # NLog Adapter (net8.0)
│ ├── EnvoyConfig.Adapters.NLog.csproj # References EnvoyConfig
│ └── NLogLoggerAdapter.cs
│
├── EnvoyConfig.Tests/ # Test project (net8.0)
│ ├── EnvoyConfig.Tests.csproj # References EnvoyConfig
│ └── ConfigLoadingTests.cs
│
└── EnvoyConfig.Sample/ # Sample project (net8.0)
├── EnvoyConfig.Sample.csproj # References EnvoyConfig
└── Program.cs
```

### 3.2. Rationale

- **Single Core Project**: `EnvoyConfig` contains the public API (`EnvConfig.Load`), runtime reflection logic, **and** the necessary definitions (`[Env]`, `IEnvLogSink`, `EnvLogLevel`). Targets `net8.0`. This simplifies the solution structure and dependencies compared to the source generator approach.
- **Adapter Projects**: Isolate logging dependencies. Now reference the main `EnvoyConfig` project. Target `net8.0`.
- **Sample/Test Projects**: Demonstrate/verify functionality. Now reference the main `EnvoyConfig` project. Target `net8.0`.

## 4. Component Details

This section details the purpose, key files, dependencies, and implementation notes for each project.

### 4.1. `EnvoyConfig` (Core Library) (net8.0)

- **Purpose:** Provides the public API (`EnvConfig.Load`) for loading configuration, manages global settings (prefix, logger), performs runtime reflection-based initialization via `Internal.ReflectionHelper`, and defines the core attribute (`EnvAttribute`) and logging interfaces (`IEnvLogSink`, `EnvLogLevel`).
- **Dependencies:** None beyond the `net8.0` framework (implicitly includes `System.Reflection`).
- **Key Files:**

  - **`EnvoyConfig.csproj`:**

  - **`Attributes/EnvAttribute.cs`:** (Namespace: `EnvoyConfig.Attributes`)

  - **`Logging/IEnvLogSink.cs`:** (Namespace: `EnvoyConfig.Logging`)

  - **`Logging/EnvLogLevel.cs`:** (Namespace: `EnvoyConfig.Logging`)

  - **`EnvConfig.cs`:** (Namespace: `EnvoyConfig`)

### 4.2. Logging Adapters (`EnvoyConfig.Adapters.*`) (net8.0)

- **Purpose:** Bridge `EnvoyConfig.Logging.IEnvLogSink` to specific logging frameworks (MEL, Serilog, NLog).
- **Dependencies:** The specific logging framework library + `EnvoyConfig`.
- **Project Files (`.csproj`):** Need `<ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />`.
- **Code Files (`*LoggerAdapter.cs`):** Need `using EnvoyConfig.Logging;`. Logic remains the same (map levels, call underlying logger).

  - **`EnvoyConfig.Adapters.Microsoft.csproj` (Example):**

  - **`MicrosoftLoggerAdapter.cs` (Example):**

  - *(Similar structure for Serilog and NLog adapters)*

### 4.3. `EnvoyConfig.Tests` (net8.0)

- **Purpose:** Unit/Integration tests for the reflection-based loading logic, type parsing, prefix handling, logging, and different attribute modes (including Dictionary and Array).
- **Dependencies:** `MSTest.*`, `Moq`, `EnvoyConfig`.
- **Project File (`.csproj`):** Needs `<ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />`. No analyzer reference needed.

    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <LangVersion>latest</LangVersion>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
        <PackageReference Include="Moq" Version="4.20.70" />
        <PackageReference Include="MSTest.TestAdapter" Version="3.2.2" />
        <PackageReference Include="MSTest.TestFramework" Version="3.2.2" />
        <PackageReference Include="coverlet.collector" Version="6.0.2">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
      </ItemGroup>
      <ItemGroup>
        <!-- Reference the Core EnvoyConfig library -->
        <ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />
      </ItemGroup>
    </Project>
    ```

- **`ConfigLoadingTests.cs`:** Adapt test cases. Remove assumptions about generated code. Focus on setting environment variables, calling `EnvConfig.Load<T>`, and asserting the state of the returned object. Use Moq to verify `IEnvLogSink` calls for errors/warnings. Add tests for `T[]` and `Dictionary<K,V>` modes.

### 4.4. `EnvoyConfig.Sample` (net8.0)

- **Purpose:** Demonstrates typical usage of the reflection-based library.
- **Dependencies:** `EnvoyConfig`, one adapter (e.g., `EnvoyConfig.Adapters.Microsoft`), corresponding logger package (`Microsoft.Extensions.Logging.Console`).
- **Project File (`.csproj`):** Needs `<ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />` and the chosen adapter reference. No analyzer reference.

- **`Program.cs`:** Define sample config classes using `[Env]`. Add examples for Arrays and Dictionaries. Set up logging. Set environment variables. Call `EnvConfig.Load<T>`. Print results. Use `using EnvoyConfig.Attributes;` and `using EnvoyConfig.Logging;`.

## 5. Build, Packaging, and CI/CD

### 5.1. Build Process

- Uses standard `dotnet build EnvoyConfig.sln`.
- No source generator execution step involved. The build compiles the C# code directly.
- Output assemblies are placed in `bin/[Configuration]/net8.0/`.

### 5.2. Testing

- Uses standard `dotnet test EnvoyConfig.sln`.
- Executes MSTest tests defined in `EnvoyConfig.Tests`.
- Coverage collection via `coverlet` remains the same (`dotnet test --collect:"XPlat Code Coverage"`).
- Add cross-platform tests for Windows vs Linux environment-variable key-casing behavior.

### 5.3. Packaging (`dotnet pack`)

- Creates NuGet packages (`.nupkg`) for `EnvoyConfig`, `EnvoyConfig.Adapters.Microsoft`, `EnvoyConfig.Adapters.Serilog`, `EnvoyConfig.Adapters.NLog`.
- Uses metadata defined in the `.csproj` files.
- No special analyzer packaging is required.
- Command: `dotnet pack EnvoyConfig.sln --configuration Release --no-build --output ./artifacts/packages`
- Validate NuGet package contents post-pack (e.g. ensure no stray DLLs or unexpected files).

### 5.4. CI/CD

- **CI (GitHub Actions, etc.):**
  1. Checkout code.
  2. Setup .NET SDK (8.x).
  3. Restore dependencies (`dotnet restore`).
  4. Build solution (`dotnet build --configuration Release --no-restore`).
  5. Run tests (`dotnet test --configuration Release --no-build`).
  6. **Build Documentation (`docfx build docs/docfx.json` or `dotnet docfx build docs/docfx.json`).**

- **CD / Release:**
  1. Perform CI steps.
  2. Determine version using **GitVersion** (reads Git history to produce SemVer 2.0 compatible versions).
  3. Pack NuGet packages (`dotnet pack ... /p:Version=[GitVersionNumber]`).
  4. Publish packages (`dotnet nuget push`).
  5. **Publish Documentation (e.g., deploy `docs/_site` to GitHub Pages).**
  6. Create Git tag and GitHub Release.

## 6. Documentation

### 6.1. `README.md` Outline (Solution Root)

1. **Title & Badges:** EnvoyConfig, Build Status, NuGet versions (Core, Adapters), License, Coverage.
2. **Introduction:** What it is (Runtime Reflection env var config), problem solved, benefits (simplicity, type safety at runtime).
3. **Features:** Bulleted list (Attribute-based, Runtime Reflection, Type Support [incl. Array/Dictionary], Initialization Modes [Key, CommaList/Array, NumberedList/Array, DictionaryMap, Nested], Prefix Handling, Logging).
4. **Installation:**
    - Explain users install `EnvoyConfig` package (`dotnet add package EnvoyConfig`).
    - Mention `EnvAttribute` and logging interfaces are included.
    - Explain optional adapter installation (e.g., `dotnet add package EnvoyConfig.Adapters.Microsoft`).
5. **Quick Start:** Minimal example: Simple class, `[Env(Key=...)]`, env vars, `EnvConfig.Load<T>()`, access values.
6. **Advanced Usage & Features:**
    - Prefix Handling (`GlobalPrefix`, `globalPrefixOverride`).
    - Attribute Modes: Examples for *each* mode (`Key`+`Default`, `Key`+`IsList`+`ListSeparator`, `ListPrefix`, `MapPrefix`, `NestedPrefix`) showing class structure and corresponding env vars. Include Array and Dictionary examples.
    - Supported Types List.
    - Logging Integration: `IEnvLogSink`, setup example using an adapter.
7. **Troubleshooting / FAQ:** Common issues (Type conversion errors, reflection exceptions, prefix confusion, logging setup).
8. **Contributing:** Guidelines.
9. **License:** LGPL-3.0-or-later.

### 6.2. XML Documentation Comments

- Maintain comprehensive `///` comments for all public types/members in `EnvoyConfig` and adapters.
- Use `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `.csproj` files. This aids IntelliSense and potential future DocFX generation.

## 7. Implementation Plan

Overview: Steps optimized for execution via Windsurf AI using GPT-4.1, referencing blueprint sections for focused context.

1. Scaffold Core Project ([3.1 Project Structure](#31-ascii-tree-diagram), [4.1 Core Library](#41-envoyconfig-core-library-net80))
   - Create solution, core `EnvoyConfig` project & csproj
   - Add `Attributes/`, `Logging/`, `Internal/` folders and key files

2. Implement Reflection Helper ([4.1 Core Library](#41-envoyconfig-core-library-net80), [2.4 Error Handling](#24-error-handling-logging-vs-exceptions))
   - Add thread-safe metadata cache
   - Develop `PopulateInstance<T>` and mode handlers (Key, List, Map, Nested)

3. Integrate Load Method ([4.1 Core Library](#41-envoyconfig-core-library-net80), [2.4 Error Handling](#24-error-handling-logging-vs-exceptions))
   - Ensure `EnvConfig.Load<T>` invokes helper and logs per rules

4. Set Up Adapters ([4.2 Logging Adapters](#42-logging-adapters-envoyconfigadapters-net80))
   - Create Microsoft, Serilog, NLog adapter projects
   - Map `IEnvLogSink` to each framework

5. Develop Tests ([4.3 EnvoyConfig.Tests](#43-envoyconfigtests-net80))
   - Configure MSTest & Moq
   - Write unit tests for every attribute mode & logging

6. Build Sample App ([4.4 EnvoyConfig.Sample](#44-envoyconfigsample-net80))
   - Scaffold sample project, configure logging adapter
   - Demonstrate all modes with example classes & env vars

7. Prepare Documentation ([6.1 README Outline](#61-readmemd-outline-solution-root), [6.2 XML Docs](#62-xml-documentation-comments))
   - Draft `README.md` per outline
   - Ensure full XML comments in public APIs

8. Configure CI/CD ([5.1 Build Process](#51-build-process), [5.2 Testing](#52-testing), [5.3 Packaging](#53-packaging-dotnet-pack), [5.4 CI/CD](#54-cicd))
   - Define GitHub Actions for build, test, pack, publish
