# EnvoyConfig - Detailed Blueprint & Implementation Plan

**Version:** 5.0
**Variant:** Runtime Reflection, Merged Structure, .NET 8
**Date:** 2025-04-13
**Author:** Fabian Schmieder
**Company:** metaneutrons
**License:** GPL-3.0-or-later

## 0. Goals & Scope

This document provides a complete blueprint for building the EnvoyConfig library from scratch using **Runtime Reflection**. EnvoyConfig simplifies application configuration by initializing C# objects from environment variables at runtime.

The primary goal is to create a robust, maintainable, and easy-to-use library that supports various configuration patterns through environment variables, leveraging runtime reflection and modern .NET features and best practices.

**Scope:**

* Core library for loading configuration via reflection.
* Attribute definitions (`EnvAttribute`) and logging interfaces (`IEnvLogSink`) within the core library.
* Logging adapters for common frameworks.
* Sample application demonstrating usage.
* Test suite for verifying functionality.
* Packaging for NuGet distribution.

## 1. Overview

EnvoyConfig is a modern C# library designed for .NET 8 applications, simplifying configuration by automatically initializing properties from environment variables at **runtime using Reflection**. It allows developers to define strongly-typed configuration classes and populate them seamlessly from the environment.

### 1.1. Core Features

* **Attribute-Based:** Uses a flexible `[Env]` attribute (defined within `EnvoyConfig`) to mark properties for initialization based on environment variable keys or prefixes.
* **Runtime Reflection:** Leverages .NET Reflection APIs (`System.Reflection`) to discover properties with `[Env]` attributes at runtime and dynamically read/parse environment variables to populate them.
* **Type Support:** Handles common primitives (string, int, bool, double, Guid, TimeSpan, DateTime, etc.), nullable value types, enums, nested complex objects (classes/structs), `List<T>`, `T[]` arrays, and `Dictionary<TKey, TValue>` (where T, TKey, TValue are supported primitives, enums, or strings).
* **Initialization Modes:**
  * **Direct Key:** `[Env(Key = "MY_VAR")]` maps a property to a single environment variable. Supports optional `Default` value. (Supported Types: Primitives, Enums, Nullables, String).
  * **Comma-Separated List/Array:** `[Env(Key = "MY_LIST_VAR", IsList = true, ListSeparator = ',')]` maps a `List<T>` or `T[]` property (where T is primitive/enum/string) to a single delimiter-separated environment variable. Supports optional `Default` value for the *entire* list string. (Supported Types: `List<T>`, `T[]`).
  * **Numbered List Prefix:** `[Env(ListPrefix = "MY_ITEM_")]` maps a `List<T>` or `T[]` property (where T is primitive/enum/string) to environment variables named sequentially (e.g., `MY_ITEM_1`, `MY_ITEM_2`, ...) starting with index 1. Stops at the first missing index or parse error. (Supported Types: `List<T>`, `T[]`).
  * **Dictionary Key-Value Prefix:** `[Env(MapPrefix = "MY_MAP_")]` maps a `Dictionary<TKey, TValue>` property (where TKey/TValue are primitive/enum/string) to environment variables named with the prefix (e.g., `MY_MAP_Key1=Value1`, `MY_MAP_Key2=Value2`). The part after the prefix is treated as the dictionary key. (Supported Types: `Dictionary<TKey, TValue>`).
  * **Nested Object Prefix:** `[Env(NestedPrefix = "MQTT_ZONE_1_")]` applied to a complex object property (class/struct). Triggers recursive initialization of that object's properties using reflection, prepending the specified prefix to the `Key`/`ListPrefix`/`MapPrefix`/`NestedPrefix` values defined within the nested object's `[Env]` attributes. The recursion uses the same core loading logic. (Supported Types: Class, Struct).
* **Flexible Prefix Handling:** Facilitates organizing environment variables:
  * **Global Static Prefix:** `EnvConfig.GlobalPrefix` can be set once (e.g., "MYAPP\_") to be automatically prepended to all environment variable lookups.
  * **Per-Call Override Prefix:** The `EnvConfig.Load<T>(globalPrefixOverride: "...")` method allows specifying a prefix for a specific load operation, taking precedence over `EnvConfig.GlobalPrefix`.
* **Defaults & Null Handling:** Supports optional `Default` string values for `Key` and `CommaList/Array` modes. Handles missing environment variables gracefully (properties retain default C# values or become null/empty collections). Logs warnings/errors for missing required variables or failed parsing via `IEnvLogSink`.
* **Logging Integration:** Provides a simple logging abstraction (`IEnvLogSink` within `EnvoyConfig.Logging`) to allow consumers to integrate EnvoyConfig's internal operational logs (e.g., warnings about defaults, parse errors) into their application's logging framework. Adapter packages are provided.
* **Minimal Dependencies:** The core library (`EnvoyConfig`) has no external dependencies beyond the .NET 8 runtime. Consuming applications only need to reference the core library and optionally, logging adapters.
* **Performance & Caching:** Reflection metadata and attribute lookups are cached per type in a thread-safe store to optimize repeated `EnvConfig.Load<T>()` calls and ensure concurrent safety.

### 1.2. Technology Stack

* **.NET 8:** Target framework for all projects (Core, Adapters, Sample, Tests).
* **C# 12:** Utilizing modern language features where appropriate.
* **System.Reflection:** Core mechanism for discovering attributes and setting properties at runtime.
* **MSTest:** Chosen testing framework (`Microsoft.NET.Test.Sdk`, `MSTest.*` packages).
* **Moq:** Chosen mocking library for testing logging integration (`Moq` package).
* **GitVersion:** Assumed to be used externally for managing SemVer 2.0 versioning based on Git history.

## 2. Coding Conventions & Practices

Adherence to standard .NET coding conventions and modern best practices is expected.

### 2.1. Target Frameworks

* **All Projects:** `net8.0` (`EnvoyConfig`, `EnvoyConfig.Adapters.*`, `EnvoyConfig.Tests`, `EnvoyConfig.Sample`)

### 2.2. Language Features (C# 12)

* Utilize C# 12 features where they enhance clarity (e.g., file-scoped namespaces, collection expressions `[]`).
* Enable `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>` for all projects.

### 2.3. Naming & Style

* Follow standard .NET Naming Conventions (PascalCase for types/methods/properties, camelCase for locals/parameters).
* Use file-scoped namespaces (`namespace MyNamespace;`).
* Prefer `var` when the type is obvious.
* Utilize expression-bodied members for simple getters/methods.
* Adhere to formatting guidelines (use `dotnet format`).
* Add XML documentation comments (`/// <summary>...`) for all public types and members.

### 2.4. Error Handling (Logging vs Exceptions)

* **Core Library (`EnvConfig.Load<T>` & `ReflectionHelper`):**
  * Should *not* throw exceptions for common configuration issues like missing optional variables or using defaults. Log these at Debug/Warning via `IEnvLogSink`.
  * Should log an Error via `IEnvLogSink` if a required variable (non-nullable value type property without a `Default` attribute) is missing, or if a value (from environment or `Default` attribute) fails to parse.
  * In case of a parse error, the property should retain its default C# value (or `null`/empty collection).
  * *May* throw Reflection-related exceptions (`TargetInvocationException`, `MemberAccessException`, `ArgumentException` from `PropertyInfo.SetValue`, etc.) if fundamental reflection operations fail unexpectedly. These indicate potential programming errors or incompatible types.
  * Aim to catch `FormatException` during parsing and log errors via `IEnvLogSink`.
* **Adapters:** Should catch potential exceptions from the underlying logging framework during their `Log` call to prevent adapter failures from crashing the configuration process.

### 2.5. Performance & Thread-Safety

* Reflection scans (property/attribute discovery) are cached in a `ConcurrentDictionary<Type,...>` to minimize overhead on repeated loads.
* `EnvConfig.Load<T>()` and its caching layer are fully thread-safe and can be called concurrently.

### 2.5. Dependency Management

* Minimize external dependencies in the `EnvoyConfig` Core library (ideally none beyond the framework).
* Adapter libraries should only reference their specific logging framework and the `EnvoyConfig` core library.

## 3. Project Structure (Flat, Merged)

All projects reside directly under the main solution root directory. The `Abstractions` project is removed, and its contents are merged into `EnvoyConfig`.

### 3.1. ASCII Tree Diagram

Use code with caution.
Markdown
EnvoyConfig/ # Root directory (Solution Location)
├── .gitignore
├── README.md
├── LICENSE
├── EnvoyConfig.sln # Solution file
│
├── EnvoyConfig/ # Core library project (net8.0) - CONTAINS ALL LOGIC & DEFINITIONS
│ ├── EnvoyConfig.csproj
│ ├── Attributes/
│ │ └── EnvAttribute.cs # Definition moved here
│ ├── Logging/
│ │ ├── IEnvLogSink.cs # Definition moved here
│ │ └── EnvLogLevel.cs # Definition moved here
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

### 3.2. Rationale

* **Single Core Project**: `EnvoyConfig` contains the public API (`EnvConfig.Load`), runtime reflection logic, **and** the necessary definitions (`[Env]`, `IEnvLogSink`, `EnvLogLevel`). Targets `net8.0`. This simplifies the solution structure and dependencies compared to the source generator approach.
* **Adapter Projects**: Isolate logging dependencies. Now reference the main `EnvoyConfig` project. Target `net8.0`.
* **Sample/Test Projects**: Demonstrate/verify functionality. Now reference the main `EnvoyConfig` project. Target `net8.0`.

## 4. Component Details

This section details the purpose, key files, dependencies, and implementation notes for each project.

### 4.1. `EnvoyConfig` (Core Library) (net8.0)

* **Purpose:** Provides the public API (`EnvConfig.Load`) for loading configuration, manages global settings (prefix, logger), performs runtime reflection-based initialization via `Internal.ReflectionHelper`, and defines the core attribute (`EnvAttribute`) and logging interfaces (`IEnvLogSink`, `EnvLogLevel`).
* **Dependencies:** None beyond the `net8.0` framework (implicitly includes `System.Reflection`).
* **Key Files:**

  * **`EnvoyConfig.csproj`:**

        ```xml
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <LangVersion>latest</LangVersion>
            <!-- GitVersion will override this -->
            <VersionPrefix>0.0.1</VersionPrefix>
            <Title>EnvoyConfig (Runtime Reflection)</Title>
            <Description>Library for initializing configuration objects from environment variables using runtime reflection.</Description>
            <Authors>Fabian Schmieder (metaneutrons)</Authors>
            <Company>metaneutrons</Company>
            <PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression>
            <PackageReadmeFile>README.md</PackageReadmeFile>
            <PackageProjectUrl>http://github.com/metaneutrons/EnvoyConfig</PackageProjectUrl>
            <RepositoryUrl>http://github.com/metaneutrons/EnvoyConfig.git</RepositoryUrl>
            <RepositoryType>git</RepositoryType>
            <GenerateDocumentationFile>true</GenerateDocumentationFile>
            <PackageTags>configuration;environment variables;runtime;reflection;env;settings;dotnet</PackageTags>
            <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
          </PropertyGroup>

          <!-- No Project References needed for core functionality -->

          <ItemGroup>
            <None Include="..\README.md" Pack="true" PackagePath="\"/>
          </ItemGroup>
          <!-- Files in Attributes/, Logging/, Internal/ are included automatically -->

        </Project>
        ```

  * **`Attributes/EnvAttribute.cs`:** (Namespace: `EnvoyConfig.Attributes`)

        ```csharp
        namespace EnvoyConfig.Attributes;

        using System;

        /// <summary>
        /// Marks a property to be initialized from environment variables by EnvoyConfig at runtime using reflection.
        /// Only one of Key, ListPrefix, MapPrefix, or NestedPrefix should be set.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        public sealed class EnvAttribute : Attribute
        {
            /// <summary>
            /// The specific key (name) of the environment variable. Used for direct mapping or
            /// comma-separated lists/arrays (when IsList = true).
            /// Mutually exclusive with ListPrefix, MapPrefix, NestedPrefix.
            /// </summary>
            public string? Key { get; set; }

            /// <summary>
            /// The prefix for numbered environment variables (e.g., "MY_ITEM_"). Used for mapping List<T>/T[] properties
            /// to MY_ITEM_1, MY_ITEM_2, ... starting at index 1.
            /// Mutually exclusive with Key, MapPrefix, NestedPrefix. Requires property to be List<T> or T[].
            /// </summary>
            public string? ListPrefix { get; set; }

            /// <summary>
            /// The prefix for key-value environment variables (e.g., "MY_MAP_"). Used for mapping Dictionary<TKey, TValue> properties
            /// to environment variables like MY_MAP_Setting1=ValueA, MY_MAP_Setting2=ValueB.
            /// Mutually exclusive with Key, ListPrefix, NestedPrefix. Requires property to be Dictionary<TKey, TValue>.
            /// </summary>
            public string? MapPrefix { get; set; }

            /// <summary>
            /// The prefix applied when mapping variables to nested object properties (e.g., [Env(NestedPrefix = "MQTT_")]).
            /// The reflection logic combines this with the Keys/Prefixes specified on the nested object's properties during recursion.
            /// Mutually exclusive with Key, ListPrefix, MapPrefix. Requires property to be a class/struct type.
            /// </summary>
            public string? NestedPrefix { get; set; }

            /// <summary>
            /// Optional default value (as a string) if the environment variable (for Key mode) or the delimited list string (for Key+IsList mode) is not set.
            /// Ignored for ListPrefix, MapPrefix, or NestedPrefix modes.
            /// </summary>
            public string? Default { get; set; }

            /// <summary>
            /// For Key mode only: Specifies if the variable contains a delimiter-separated list/array. Ignored otherwise. Defaults to false.
            /// Requires property type to be List<T> or T[].
            /// </summary>
            public bool IsList { get; set; }

            /// <summary>
            /// Separator character for Key mode lists/arrays (when IsList = true). Defaults to ','.
            /// </summary>
            public char ListSeparator { get; set; } = ',';

            // Runtime Validation: The ReflectionHelper must validate that only one mode (Key, ListPrefix, MapPrefix, NestedPrefix) is set.
            // Runtime Validation: The ReflectionHelper must validate type compatibility (e.g., IsList requires List<T>/T[], ListPrefix requires List<T>/T[], etc.).
        }
        ```

  * **`Logging/IEnvLogSink.cs`:** (Namespace: `EnvoyConfig.Logging`)

        ```csharp
        namespace EnvoyConfig.Logging;

        using System; // Required for Exception

        /// <summary>
        /// Defines a simple abstraction for logging messages during the configuration loading process.
        /// Implement this interface to integrate EnvoyConfig logs with your application's logging framework.
        /// </summary>
        public interface IEnvLogSink
        {
            /// <summary>Logs a message.</summary>
            /// <param name="level">The severity level of the message.</param>
            /// <param name="message">The message text.</param>
            /// <param name="exception">Optional exception associated with the message.</param>
            void Log(EnvLogLevel level, string message, Exception? exception = null);
        }
        ```

  * **`Logging/EnvLogLevel.cs`:** (Namespace: `EnvoyConfig.Logging`)

        ```csharp
        namespace EnvoyConfig.Logging;

        /// <summary>Defines the levels for logging messages during configuration loading.</summary>
        public enum EnvLogLevel
        {
            Off = 0, Error = 1, Warning = 2, Info = 3, Debug = 4
        }
        ```

  * **`EnvConfig.cs`:** (Namespace: `EnvoyConfig`)

        ```csharp
        namespace EnvoyConfig;

        using EnvoyConfig.Internal; // For ReflectionHelper
        using EnvoyConfig.Logging;  // For IEnvLogSink, EnvLogLevel
        using System;
        using System.Diagnostics; // For potential future Debug.Assert

        /// <summary>
        /// Provides global configuration settings and the entry point for loading configuration objects using runtime reflection.
        /// </summary>
        public static class EnvConfig
        {
            private static IEnvLogSink? _logger;
            private static string _globalPrefix = string.Empty;

            /// <summary>Gets or sets the log sink used by EnvoyConfig during loading. Set before calling Load<T>().</summary>
            public static IEnvLogSink? Logger { get => _logger; set => _logger = value; }

            /// <summary>Gets or sets the minimum log level for messages processed by EnvoyConfig. Defaults to Error.</summary>
            public static EnvLogLevel LogLevel { get; set; } = EnvLogLevel.Error;

            /// <summary>Gets or sets a global prefix automatically prepended to all environment variable lookups. Defaults to empty string.</summary>
            public static string GlobalPrefix { get => _globalPrefix; set => _globalPrefix = value ?? string.Empty; }

            /// <summary>Internal logging helper, respecting LogLevel and handling potential logger errors.</summary>
            internal static void LogInternal(EnvLogLevel level, string message, Exception? ex = null)
            {
                if (level > LogLevel || Logger == null) return;
                try { Logger.Log(level, message, ex); }
                catch (Exception logEx) { System.Diagnostics.Debug.WriteLine($"EnvoyConfig: Logger failed: {logEx.Message}"); }
            }

            /// <summary>
            /// Loads and initializes configuration from environment variables for the specified type T using runtime reflection.
            /// </summary>
            /// <typeparam name="T">The configuration class type (must have a parameterless constructor for EnvoyConfig to instantiate if needed, especially for nested types).</typeparam>
            /// <param name="globalPrefixOverride">Optional. If provided, this prefix is used instead of EnvConfig.GlobalPrefix for this specific load operation.</param>
            /// <returns>An initialized instance of T.</returns>
            /// <remarks>
            /// This method uses reflection to find properties decorated with <see cref="Attributes.EnvAttribute"/>.
            /// It reads corresponding environment variables, parses them, and sets the property values.
            /// Errors during parsing or for missing required variables are logged via the configured <see cref="Logger"/>.
            /// Reflection errors might still be thrown.
            /// </remarks>
            public static T Load<T>(string? globalPrefixOverride = null) where T : new()
            {
                string effectivePrefix = globalPrefixOverride ?? GlobalPrefix;
                LogInternal(EnvLogLevel.Debug, $"Loading config for {typeof(T).FullName} using reflection with effective prefix '{effectivePrefix}'.");

                T configInstance = new T(); // Create the root instance

                try
                {
                    // Delegate the core reflection work to the helper class
                    ReflectionHelper.PopulateInstance(configInstance, effectivePrefix, Logger, LogLevel);
                    LogInternal(EnvLogLevel.Info, $"Successfully populated configuration instance for type {typeof(T).FullName}.");
                }
                catch (Exception ex) // Catch potential reflection exceptions from the helper
                {
                    LogInternal(EnvLogLevel.Error, $"Critical error during reflection-based configuration loading for {typeof(T).FullName}: {ex.Message}", ex);
                    // Re-throw or wrap? Re-throwing preserves original stack trace.
                    // Consider wrapping in a specific EnvoyConfigLoadException if needed.
                    throw;
                }

                return configInstance;
            }
        }
        ```

  * **`Internal/ReflectionHelper.cs`:** (Namespace: `EnvoyConfig.Internal`)
    * **Purpose:** Encapsulates the core reflection logic for finding attributes, reading environment variables, parsing values, handling different modes (Key, Lists, Map, Nested), and setting properties.
    * **Implementation Sketch:**

            ```csharp
            using EnvoyConfig.Attributes;
            using EnvoyConfig.Logging;
            using System;
            using System.Collections;
            using System.Collections.Generic;
            using System.ComponentModel;
            using System.Globalization;
            using System.Linq;
            using System.Reflection;

            internal static class ReflectionHelper
            {
                // Main entry point called by EnvConfig.Load
                public static void PopulateInstance<T>(T instance, string effectivePrefix, IEnvLogSink? logger, EnvLogLevel configuredLogLevel)
                {
                    if (instance == null) return; // Should not happen if called from Load<T> where T:new()

                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(p => p.CanWrite); // Only consider properties with a public setter

                    foreach (var propInfo in properties)
                    {
                        var envAttr = propInfo.GetCustomAttribute<EnvAttribute>();
                        if (envAttr == null) continue; // Skip properties without the attribute

                        // --- Runtime Validation of Attribute Usage ---
                        int modeCount = (envAttr.Key != null ? 1 : 0) +
                                        (envAttr.ListPrefix != null ? 1 : 0) +
                                        (envAttr.MapPrefix != null ? 1 : 0) +
                                        (envAttr.NestedPrefix != null ? 1 : 0);

                        if (modeCount != 1)
                        {
                            Log(logger, configuredLogLevel, EnvLogLevel.Error, $"Invalid [Env] attribute on {typeof(T).Name}.{propInfo.Name}. Specify exactly one of Key, ListPrefix, MapPrefix, or NestedPrefix.", null);
                            continue; // Skip this property
                        }

                        // --- Mode Dispatch ---
                        try
                        {
                            if (envAttr.Key != null)
                            {
                                ProcessKeyMode(instance, propInfo, envAttr, effectivePrefix, logger, configuredLogLevel);
                            }
                            else if (envAttr.ListPrefix != null)
                            {
                                ProcessNumberedListMode(instance, propInfo, envAttr, effectivePrefix, logger, configuredLogLevel);
                            }
                            else if (envAttr.MapPrefix != null)
                            {
                                ProcessMapMode(instance, propInfo, envAttr, effectivePrefix, logger, configuredLogLevel);
                            }
                            else // NestedPrefix must be non-null
                            {
                                ProcessNestedMode(instance, propInfo, envAttr, effectivePrefix, logger, configuredLogLevel);
                            }
                        }
                        catch (Exception ex) // Catch errors during processing for a single property
                        {
                             Log(logger, configuredLogLevel, EnvLogLevel.Error, $"Error processing property {typeof(T).Name}.{propInfo.Name}: {ex.Message}", ex);
                             // Continue processing other properties
                        }
                    }
                }

                // --- Mode Processing Methods (Example Stubs) ---

                private static void ProcessKeyMode(object instance, PropertyInfo propInfo, EnvAttribute attr, string prefix, IEnvLogSink? logger, EnvLogLevel configLevel)
                {
                    // Validate IsList type compatibility
                    bool isListMode = attr.IsList;
                    Type propType = propInfo.PropertyType;
                    bool isGenericList = propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>);
                    bool isArray = propType.IsArray && propType.GetArrayRank() == 1;

                    if (isListMode && !(isGenericList || isArray)) {
                        Log(logger, configLevel, EnvLogLevel.Error, $"[Env(IsList=true)] requires List<T> or T[] type for property {instance.GetType().Name}.{propInfo.Name}, but found {propType.Name}.", null);
                        return;
                    }
                     if (!isListMode && (isGenericList || isArray)) {
                        // Maybe allow direct key mapping to a single-element list/array? Or log error? For now, log error.
                         Log(logger, configLevel, EnvLogLevel.Error, $"[Env(Key=...)] without IsList=true cannot be applied to List<T> or T[] property {instance.GetType().Name}.{propInfo.Name}. Use IsList=true for comma-separated value or ListPrefix/MapPrefix.", null);
                        return;
                    }

                    string fullKey = string.IsNullOrEmpty(prefix) ? attr.Key! : $"{prefix}{attr.Key!}";
                    string? envValue = Environment.GetEnvironmentVariable(fullKey);
                    string? valueToParse = envValue;
                    bool usedDefault = false;

                    if (string.IsNullOrEmpty(envValue) && attr.Default != null)
                    {
                        valueToParse = attr.Default;
                        usedDefault = true;
                         Log(logger, configLevel, EnvLogLevel.Debug, $"Key '{fullKey}' not found or empty for {propInfo.Name}. Using default value '{valueToParse}'.", null);
                    }

                    if (valueToParse != null)
                    {
                        object? parsedValue = null;
                        if (isListMode) {
                            // Logic to split valueToParse by attr.ListSeparator
                            // Get element type T from List<T> or T[]
                            // Parse each part into T
                            // Create List<T> or T[] instance
                            // parsedValue = the created list/array
                             Type elementType = isGenericList ? propType.GetGenericArguments()[0] : propType.GetElementType()!;
                             parsedValue = ParseListOrArray(valueToParse, attr.ListSeparator, elementType, propType.IsArray, logger, configLevel, fullKey, propInfo.Name);
                        } else {
                            // Logic to parse single valueToParse into propInfo.PropertyType
                             parsedValue = ParseValue(valueToParse, propInfo.PropertyType, logger, configLevel, fullKey, propInfo.Name);
                        }

                        if (parsedValue != null) {
                            try { propInfo.SetValue(instance, parsedValue); }
                            catch(ArgumentException argEx) { Log(logger, configLevel, EnvLogLevel.Error, $"Type mismatch setting property {propInfo.Name} from key '{fullKey}'. Expected {propInfo.PropertyType}, got {parsedValue.GetType()}. {argEx.Message}", argEx); }
                        } else {
                             // Error already logged by ParseValue/ParseListOrArray if parsing failed
                             // If valueToParse was null/empty and no default, log if needed
                             if (envValue == null && !usedDefault) CheckAndLogIfRequiredMissing(propInfo, logger, configLevel, fullKey);
                        }
                    } else {
                        // Value not found, no default provided
                        CheckAndLogIfRequiredMissing(propInfo, logger, configLevel, fullKey);
                    }
                }

                 private static void ProcessNumberedListMode(object instance, PropertyInfo propInfo, EnvAttribute attr, string prefix, IEnvLogSink? logger, EnvLogLevel configLevel)
                 {
                    // Validate property type is List<T> or T[]
                    Type propType = propInfo.PropertyType;
                    bool isGenericList = propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>);
                    bool isArray = propType.IsArray && propType.GetArrayRank() == 1;
                    if (!(isGenericList || isArray)) {
                         Log(logger, configLevel, EnvLogLevel.Error, $"[Env(ListPrefix=...)] requires List<T> or T[] type for property {instance.GetType().Name}.{propInfo.Name}, but found {propType.Name}.", null);
                         return;
                    }
                    Type elementType = isGenericList ? propType.GetGenericArguments()[0] : propType.GetElementType()!;

                    var list = new ArrayList(); // Use ArrayList to dynamically add items
                    for (int i = 1; ; i++)
                    {
                        string itemKey = $"{prefix}{attr.ListPrefix}{i}";
                        string? itemValue = Environment.GetEnvironmentVariable(itemKey);

                        if (string.IsNullOrEmpty(itemValue)) {
                            if (i==1) Log(logger, configLevel, EnvLogLevel.Debug, $"Numbered list start key '{itemKey}' not found for {propInfo.Name}. List will be empty.", null);
                            break; // Stop at first missing index
                        }

                        object? parsedItem = ParseValue(itemValue, elementType, logger, configLevel, itemKey, $"{propInfo.Name}[{i}]");
                        if (parsedItem != null) {
                            list.Add(parsedItem);
                        } else {
                            Log(logger, configLevel, EnvLogLevel.Warning, $"Parse failed for numbered list item '{itemKey}' for {propInfo.Name}. Stopping list population.", null);
                            break; // Stop on first parse error
                        }
                    }

                     // Convert ArrayList to the target type (List<T> or T[])
                    object? finalCollection = null;
                    if(isGenericList) {
                        var typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), list.Count)!;
                        foreach(var item in list) { typedList.Add(item); }
                        finalCollection = typedList;
                    } else { // isArray
                         var typedArray = Array.CreateInstance(elementType, list.Count);
                         list.CopyTo(typedArray, 0);
                         finalCollection = typedArray;
                    }

                     try { propInfo.SetValue(instance, finalCollection); }
                     catch(ArgumentException argEx) { Log(logger, configLevel, EnvLogLevel.Error, $"Type mismatch setting property {propInfo.Name} from ListPrefix '{attr.ListPrefix}'. {argEx.Message}", argEx); }
                 }


                private static void ProcessMapMode(object instance, PropertyInfo propInfo, EnvAttribute attr, string prefix, IEnvLogSink? logger, EnvLogLevel configLevel)
                {
                    // Validate property type is Dictionary<TKey, TValue>
                     Type propType = propInfo.PropertyType;
                     if (!propType.IsGenericType || propType.GetGenericTypeDefinition() != typeof(Dictionary<,>)) {
                         Log(logger, configLevel, EnvLogLevel.Error, $"[Env(MapPrefix=...)] requires Dictionary<TKey, TValue> type for property {instance.GetType().Name}.{propInfo.Name}, but found {propType.Name}.", null);
                         return;
                     }
                     Type keyType = propType.GetGenericArguments()[0];
                     Type valueType = propType.GetGenericArguments()[1];

                     // Create dictionary instance
                    var dictionary = (IDictionary)Activator.CreateInstance(propType)!;
                    string mapPrefix = $"{prefix}{attr.MapPrefix}";

                    // Iterate through *all* environment variables to find matches
                    // This can be slow if there are many variables. Consider optimizations if needed.
                    var environmentVariables = Environment.GetEnvironmentVariables();
                    foreach (DictionaryEntry entry in environmentVariables)
                    {
                        string envKey = (string)entry.Key;
                        if (envKey.StartsWith(mapPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            string dictKeyString = envKey.Substring(mapPrefix.Length);
                            string? dictValueString = entry.Value as string;

                            if (string.IsNullOrEmpty(dictKeyString) || dictValueString == null) continue; // Skip if key part is empty or value is null

                             object? parsedKey = ParseValue(dictKeyString, keyType, logger, configLevel, envKey, $"{propInfo.Name}[Key]");
                             object? parsedValue = ParseValue(dictValueString, valueType, logger, configLevel, envKey, $"{propInfo.Name}[Value]");

                             if (parsedKey != null && parsedValue != null) {
                                 try { dictionary.Add(parsedKey, parsedValue); }
                                 catch (ArgumentException ex) { Log(logger, configLevel, EnvLogLevel.Warning, $"Failed to add key '{dictKeyString}' to dictionary for {propInfo.Name} (prefix '{mapPrefix}'). Key might already exist or be invalid. {ex.Message}", null); }
                             } else {
                                 // Error already logged by ParseValue
                                 Log(logger, configLevel, EnvLogLevel.Warning, $"Skipping entry for env var '{envKey}' due to key/value parse failure for dictionary {propInfo.Name}.", null);
                             }
                        }
                    }

                     try { propInfo.SetValue(instance, dictionary); }
                     catch(ArgumentException argEx) { Log(logger, configLevel, EnvLogLevel.Error, $"Type mismatch setting property {propInfo.Name} from MapPrefix '{attr.MapPrefix}'. {argEx.Message}", argEx); }
                }

                private static void ProcessNestedMode(object instance, PropertyInfo propInfo, EnvAttribute attr, string prefix, IEnvLogSink? logger, EnvLogLevel configLevel)
                {
                    // Get or create instance of nested object
                    object? nestedInstance = propInfo.GetValue(instance);
                    if (nestedInstance == null)
                    {
                        // Requires parameterless constructor on nested type
                        try { nestedInstance = Activator.CreateInstance(propInfo.PropertyType); }
                        catch (Exception ex) {
                             Log(logger, configLevel, EnvLogLevel.Error, $"Failed to create instance of nested type {propInfo.PropertyType.Name} for property {propInfo.Name}. It needs a parameterless constructor. {ex.Message}", ex);
                            return;
                        }
                        // Set the newly created instance back onto the parent
                        try { propInfo.SetValue(instance, nestedInstance); }
                        catch (Exception ex) { Log(logger, configLevel, EnvLogLevel.Error, $"Failed to set created nested instance for property {propInfo.Name}. {ex.Message}", ex); return; }
                    }

                    // Recursive call with combined prefix
                    string nestedPrefix = $"{prefix}{attr.NestedPrefix}";
                    Log(logger, configLevel, EnvLogLevel.Debug, $"Recursively populating nested property {propInfo.Name} with prefix '{nestedPrefix}'", null);

                    // Use reflection to call PopulateInstance dynamically for the nested type T
                    // MethodInfo populateMethod = typeof(ReflectionHelper).GetMethod(nameof(PopulateInstance), BindingFlags.Static | BindingFlags.Public); // Or NonPublic if needed
                    // MethodInfo genericPopulateMethod = populateMethod.MakeGenericMethod(propInfo.PropertyType);
                    // genericPopulateMethod.Invoke(null, new object?[] { nestedInstance, nestedPrefix, logger, configLevel });
                     // Simpler: Just call it directly since we know the type at compile time within this method context via propInfo.PropertyType
                     // BUT, PopulateInstance<T> needs a T constraint. Can we do this? Maybe need a non-generic helper or dynamic.
                     // Let's try dynamic (less performant but simpler code):
                     PopulateInstanceDynamic(nestedInstance, nestedPrefix, logger, configLevel);

                     // Alternative: Non-generic helper
                     // PopulateInstanceInternal(nestedInstance, propInfo.PropertyType, nestedPrefix, logger, configLevel);
                }

                 // Need a way to call PopulateInstance without knowing T at compile time for recursion
                 private static void PopulateInstanceDynamic(object instance, string effectivePrefix, IEnvLogSink? logger, EnvLogLevel configuredLogLevel)
                 {
                     // This uses dynamic dispatch which might be less performant
                     // It essentially calls PopulateInstance<ActualType>(...)
                     // Ensure PopulateInstance is public static if using dynamic this way from external call site if needed
                     // (though here it's internal static calling itself essentially via dynamic)
                     // This assumes T:new() constraint isn't strictly needed for the *population* part, only for creation in Load<T>
                     // Or we add a non-generic internal helper.
                     dynamic dynamicInstance = instance;
                     PopulateInstance(dynamicInstance, effectivePrefix, logger, configuredLogLevel);

                     // TODO: Re-evaluate if a non-generic internal helper is better than dynamic.
                 }


                // --- Parsing Helpers ---

                private static object? ParseValue(string value, Type targetType, IEnvLogSink? logger, EnvLogLevel configLevel, string keyContext, string propContext)
                {
                    // Handle nullables
                    Type? underlyingType = Nullable.GetUnderlyingType(targetType);
                    Type typeToConvert = underlyingType ?? targetType;

                    // Optimization: If target is string, return directly
                    if (typeToConvert == typeof(string)) return value;

                    // Use TypeConverter for broader support (handles enums, primitives, Guid, TimeSpan, DateTime etc.)
                    var converter = TypeDescriptor.GetConverter(typeToConvert);
                    if (converter != null && converter.CanConvertFrom(typeof(string)))
                    {
                        try
                        {
                             // Use CultureInfo.InvariantCulture for consistent parsing
                             object? result = converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);

                             // Handle special bool cases ("1", "0") if TypeConverter doesn't already
                             if (typeToConvert == typeof(bool) && result is bool b && !b) {
                                 if (value.Trim() == "1") return true;
                                 // Standard converter usually handles "true"/"false"
                             }
                             // Log successful parse? Maybe too verbose for Debug.
                             return result;
                        }
                        catch (Exception ex) // Catches FormatException, NotSupportedException etc. from ConvertFromString
                        {
                            Log(logger, configLevel, EnvLogLevel.Error, $"Failed to parse value '{value}' for key '{keyContext}' as type '{typeToConvert.Name}' for property '{propContext}'. {ex.Message}", null); // Don't log exception stack trace unless Debug maybe?
                            return null;
                        }
                    }

                     Log(logger, configLevel, EnvLogLevel.Error, $"No TypeConverter found to parse value '{value}' for key '{keyContext}' as type '{typeToConvert.Name}' for property '{propContext}'.", null);
                     return null; // Parsing failed
                }

                private static object? ParseListOrArray(string listString, char separator, Type elementType, bool returnAsArray, IEnvLogSink? logger, EnvLogLevel configLevel, string keyContext, string propContext)
                {
                     var items = new ArrayList();
                     string[] parts = listString.Split(separator, StringSplitOptions.RemoveEmptyEntries); // Remove empty allows "a,,b" -> ["a","b"]

                     for(int i=0; i < parts.Length; i++)
                     {
                         string trimmedPart = parts[i].Trim();
                         if (!string.IsNullOrEmpty(trimmedPart))
                         {
                             object? parsedItem = ParseValue(trimmedPart, elementType, logger, configLevel, $"{keyContext}[{i}]", $"{propContext}[{i}]");
                             if (parsedItem != null) {
                                 items.Add(parsedItem);
                             } else {
                                 // Error already logged by ParseValue
                                 // Optionally stop processing list on first item error? Currently continues.
                                 Log(logger, configLevel, EnvLogLevel.Warning, $"Skipping item at index {i} in comma-separated list for key '{keyContext}' due to parse error.", null);
                             }
                         }
                     }

                     // Convert ArrayList to the target type (List<T> or T[])
                     if(returnAsArray) {
                         var typedArray = Array.CreateInstance(elementType, items.Count);
                         items.CopyTo(typedArray, 0);
                         return typedArray;
                     } else { // Return as List<T>
                         var typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), items.Count)!;
                         foreach(var item in items) { typedList.Add(item); }
                         return typedList;
                     }
                }

                // --- Logging Helper ---
                private static void Log(IEnvLogSink? logger, EnvLogLevel configuredLevel, EnvLogLevel messageLevel, string message, Exception? ex) {
                    if (messageLevel <= configuredLevel && logger != null) {
                         try { logger.Log(messageLevel, message, ex); } catch { /* Ignore logger errors */ }
                    }
                }

                // --- Required Check Helper ---
                 private static void CheckAndLogIfRequiredMissing(PropertyInfo propInfo, IEnvLogSink? logger, EnvLogLevel configLevel, string keyContext)
                 {
                     // Required = Value Type AND NOT Nullable<T>
                     bool isRequired = propInfo.PropertyType.IsValueType && Nullable.GetUnderlyingType(propInfo.PropertyType) == null;
                     if (isRequired) {
                          Log(logger, configLevel, EnvLogLevel.Error, $"Required environment variable '{keyContext}' was not found or is empty for non-nullable property '{propInfo.DeclaringType?.Name}.{propInfo.Name}'.", null);
                     } else {
                          Log(logger, configLevel, EnvLogLevel.Debug, $"Optional environment variable '{keyContext}' was not found for property '{propInfo.DeclaringType?.Name}.{propInfo.Name}'.", null);
                     }
                 }
            }
            ```

### 4.2. Logging Adapters (`EnvoyConfig.Adapters.*`) (net8.0)

* **Purpose:** Bridge `EnvoyConfig.Logging.IEnvLogSink` to specific logging frameworks (MEL, Serilog, NLog).
* **Dependencies:** The specific logging framework library + `EnvoyConfig`.
* **Project Files (`.csproj`):** Need `<ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />`.
* **Code Files (`*LoggerAdapter.cs`):** Need `using EnvoyConfig.Logging;`. Logic remains the same (map levels, call underlying logger).

  * **`EnvoyConfig.Adapters.Microsoft.csproj` (Example):**

        ```xml
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <LangVersion>latest</LangVersion>
            <VersionPrefix>0.0.1</VersionPrefix> <!-- Match Core -->
            <Title>EnvoyConfig Adapter for Microsoft.Extensions.Logging</Title>
            <Description>Adapter to integrate EnvoyConfig logs with Microsoft.Extensions.Logging.</Description>
            <!-- Other metadata -->
            <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
          </PropertyGroup>
          <ItemGroup>
            <!-- Reference the Core EnvoyConfig library -->
            <ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />
            <!-- Reference the logging framework -->
            <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
          </ItemGroup>
          <ItemGroup>
            <None Include="..\README.md" Pack="true" PackagePath="\"/>
          </ItemGroup>
        </Project>
        ```

  * **`MicrosoftLoggerAdapter.cs` (Example):**

        ```csharp
        namespace EnvoyConfig.Adapters.Microsoft;

        using EnvoyConfig.Logging; // Use the core logging definitions
        using MEL = global::Microsoft.Extensions.Logging; // Alias to avoid conflict
        using System;

        public class MicrosoftLoggerAdapter(MEL.ILogger logger) : IEnvLogSink
        {
            private readonly MEL.ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            public void Log(EnvLogLevel level, string message, Exception? exception = null)
            {
                MEL.LogLevel melLevel = ConvertLevel(level);
                // Check if level enabled before logging (standard MEL practice)
                if (_logger.IsEnabled(melLevel))
                {
                    // Correct overload: LogLevel, EventId, State, Exception, Formatter
                    _logger.Log(melLevel, 0, exception, "{Message}", message);
                }
            }

            private static MEL.LogLevel ConvertLevel(EnvLogLevel level) => level switch
            {
                EnvLogLevel.Debug => MEL.LogLevel.Debug,
                EnvLogLevel.Info => MEL.LogLevel.Information,
                EnvLogLevel.Warning => MEL.LogLevel.Warning,
                EnvLogLevel.Error => MEL.LogLevel.Error,
                EnvLogLevel.Off => MEL.LogLevel.None,
                _ => MEL.LogLevel.None,
            };
        }
        ```

  * *(Similar structure for Serilog and NLog adapters)*

### 4.3. `EnvoyConfig.Tests` (net8.0)

* **Purpose:** Unit/Integration tests for the reflection-based loading logic, type parsing, prefix handling, logging, and different attribute modes (including Dictionary and Array).
* **Dependencies:** `MSTest.*`, `Moq`, `EnvoyConfig`.
* **Project File (`.csproj`):** Needs `<ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />`. No analyzer reference needed.

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

* **`ConfigLoadingTests.cs`:** Adapt test cases. Remove assumptions about generated code. Focus on setting environment variables, calling `EnvConfig.Load<T>`, and asserting the state of the returned object. Use Moq to verify `IEnvLogSink` calls for errors/warnings. Add tests for `T[]` and `Dictionary<K,V>` modes.

### 4.4. `EnvoyConfig.Sample` (net8.0)

* **Purpose:** Demonstrates typical usage of the reflection-based library.
* **Dependencies:** `EnvoyConfig`, one adapter (e.g., `EnvoyConfig.Adapters.Microsoft`), corresponding logger package (`Microsoft.Extensions.Logging.Console`).
* **Project File (`.csproj`):** Needs `<ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />` and the chosen adapter reference. No analyzer reference.

    ```xml
     <Project Sdk="Microsoft.NET.Sdk">
       <PropertyGroup>
         <OutputType>Exe</OutputType>
         <TargetFramework>net8.0</TargetFramework>
         <ImplicitUsings>enable</ImplicitUsings>
         <Nullable>enable</Nullable>
         <LangVersion>latest</LangVersion>
         <IsPackable>false</IsPackable>
       </PropertyGroup>
       <ItemGroup>
         <!-- Reference the Core EnvoyConfig library -->
         <ProjectReference Include="..\EnvoyConfig\EnvoyConfig.csproj" />
         <!-- Reference a Logging Adapter -->
         <ProjectReference Include="..\EnvoyConfig.Adapters.Microsoft\EnvoyConfig.Adapters.Microsoft.csproj" />
       </ItemGroup>
       <ItemGroup>
         <!-- Add necessary logging packages for the chosen adapter -->
         <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
       </ItemGroup>
     </Project>
    ```

* **`Program.cs`:** Define sample config classes using `[Env]`. Add examples for Arrays and Dictionaries. Set up logging. Set environment variables. Call `EnvConfig.Load<T>`. Print results. Use `using EnvoyConfig.Attributes;` and `using EnvoyConfig.Logging;`.

## 5. Build, Packaging, and CI/CD

### 5.1. Build Process

* Uses standard `dotnet build EnvoyConfig.sln`.
* No source generator execution step involved. The build compiles the C# code directly.
* Output assemblies are placed in `bin/[Configuration]/net8.0/`.

### 5.2. Testing

* Uses standard `dotnet test EnvoyConfig.sln`.
* Executes MSTest tests defined in `EnvoyConfig.Tests`.
* Coverage collection via `coverlet` remains the same (`dotnet test --collect:"XPlat Code Coverage"`).
* Add cross-platform tests for Windows vs Linux environment-variable key-casing behavior.

### 5.3. Packaging (`dotnet pack`)

* Creates NuGet packages (`.nupkg`) for `EnvoyConfig`, `EnvoyConfig.Adapters.Microsoft`, `EnvoyConfig.Adapters.Serilog`, `EnvoyConfig.Adapters.NLog`.
* Uses metadata defined in the `.csproj` files.
* No special analyzer packaging is required.
* Command: `dotnet pack EnvoyConfig.sln --configuration Release --no-build --output ./artifacts/packages`
* Validate NuGet package contents post-pack (e.g. ensure no stray DLLs or unexpected files).

### 5.4. CI/CD

* **CI (GitHub Actions, etc.):**
    1. Checkout code.
    2. Setup .NET SDK (8.x).
    3. Restore dependencies (`dotnet restore`).
    4. Build solution (`dotnet build --configuration Release --no-restore`).
    5. Run tests (`dotnet test --configuration Release --no-build`).
* **CD / Release:**
    1. Perform CI steps.
    2. Determine version using **GitVersion** (reads Git history to produce SemVer 2.0 compatible versions).
    3. Pack NuGet packages (`dotnet pack ... /p:Version=[GitVersionNumber]`).
    4. Publish packages (`dotnet nuget push`).
    5. Create Git tag and GitHub Release.

## 6. Documentation

### 6.1. `README.md` Outline (Solution Root)

1. **Title & Badges:** EnvoyConfig, Build Status, NuGet versions (Core, Adapters), License, Coverage.
2. **Introduction:** What it is (Runtime Reflection env var config), problem solved, benefits (simplicity, type safety at runtime).
3. **Features:** Bulleted list (Attribute-based, Runtime Reflection, Type Support [incl. Array/Dictionary], Initialization Modes [Key, CommaList/Array, NumberedList/Array, DictionaryMap, Nested], Prefix Handling, Logging).
4. **Installation:**
    * Explain users install `EnvoyConfig` package (`dotnet add package EnvoyConfig`).
    * Mention `EnvAttribute` and logging interfaces are included.
    * Explain optional adapter installation (e.g., `dotnet add package EnvoyConfig.Adapters.Microsoft`).
5. **Quick Start:** Minimal example: Simple class, `[Env(Key=...)]`, env vars, `EnvConfig.Load<T>()`, access values.
6. **Advanced Usage & Features:**
    * Prefix Handling (`GlobalPrefix`, `globalPrefixOverride`).
    * Attribute Modes: Examples for *each* mode (`Key`+`Default`, `Key`+`IsList`+`ListSeparator`, `ListPrefix`, `MapPrefix`, `NestedPrefix`) showing class structure and corresponding env vars. Include Array and Dictionary examples.
    * Supported Types List.
    * Logging Integration: `IEnvLogSink`, setup example using an adapter.
7. **Troubleshooting / FAQ:** Common issues (Type conversion errors, reflection exceptions, prefix confusion, logging setup).
8. **Contributing:** Guidelines.
9. **License:** GPL-3.0-or-later.

### 6.2. XML Documentation Comments

* Maintain comprehensive `///` comments for all public types/members in `EnvoyConfig` and adapters.
* Use `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `.csproj` files. This aids IntelliSense and potential future DocFX generation.

## 7. Implementation Plan (High-Level for Reflection)

1. **Setup Core Project:** Create `EnvoyConfig` project, move `EnvAttribute`, `IEnvLogSink`, `EnvLogLevel` into `Attributes/` and `Logging/` folders. Create `EnvConfig.cs` structure.
2. **Implement `ReflectionHelper.PopulateInstance`:** Write the core recursive reflection logic.
    * Implement a thread-safe caching layer for reflection metadata and attribute lookups.
    * Design parsing via an `IEnvValueParser` (or similar) extension point so custom `TypeConverter`-style hooks can be injected later.
    * Implement property iteration and attribute finding.
    * Implement runtime validation of attribute modes.
    * Implement `ProcessKeyMode` (including `IsList` for List/Array).
    * Implement `ProcessNumberedListMode` (for List/Array).
    * Implement `ProcessMapMode` (for Dictionary).
    * Implement `ProcessNestedMode` (recursive call).
    * Implement robust `ParseValue` helper using `TypeConverter`.
    * Implement `ParseListOrArray` helper.
    * Implement logging calls throughout.
3. **Implement `EnvConfig.Load<T>`:** Call `ReflectionHelper.PopulateInstance`.
4. **Setup Adapters:** Create projects, reference `EnvoyConfig`, implement adapters.
5. **Setup Tests:** Create project, reference `EnvoyConfig`, `MSTest`, `Moq`.
6. **Write Tests:** Implement comprehensive tests covering all modes, types, prefixes, defaults, errors, and logging.
7. **Setup Sample:** Create project, reference `EnvoyConfig`, adapter. Write sample code demonstrating features.
8. **Documentation:** Write `README.md` and ensure XML comments are complete.
9. **CI/CD:** Set up build/test/release pipeline using GitVersion.

* **Review Existing Code:** Always Check existing code, especially `EnvoyConfig.Sample` and `EnvoyConfig.Tests` for existing implementation. If found, analyze it and ask the user whether to update, extend, or replace before writing new code.
