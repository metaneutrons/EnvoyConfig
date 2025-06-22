# Getting Started

## Installation

Install the NuGet package:

```bash
dotnet add package EnvoyConfig
```

## Quick Example

### 1. Define your Configuration Class (`AppConfig.cs`)

```csharp
using EnvoyConfig.Attributes;
using System.Collections.Generic;

public class AppConfig
{
    [Env("APP_NAME", DefaultValue = "MyApp")]
    public string ApplicationName { get; set; }

    [Env("SERVER_PORT", DefaultValue = 8080)]
    public int Port { get; set; }

    [Env("FEATURE_FLAGS", IsKeyValue = true)]
    public Dictionary<string, string> FeatureFlags { get; set; }

    public bool IsFeatureEnabled(string featureName)
    {
        return FeatureFlags != null &&
               FeatureFlags.TryGetValue(featureName, out var value) &&
               bool.TryParse(value, out var result) && result;
    }
}
```

### 2. Prepare your Environment (e.g., `.env` file)

```env
APP_NAME=MyAwesomeService
SERVER_PORT=9001
FEATURE_FLAGS=NewUI=true;DetailedLogging=false;UseBetaAPI=true
```

### 3. Load Configuration in your Application (`Program.cs`)

```csharp
using System;
using EnvoyConfig;

public class Program
{
    public static void Main(string[] args)
    {
        EnvConfig.UseDotEnv();
        Console.WriteLine("Loading configuration...");
        try
        {
            var config = EnvConfig.Load<AppConfig>();
            Console.WriteLine($"Application Name: {config.ApplicationName}");
            Console.WriteLine($"Server Port: {config.Port}");
            Console.WriteLine("Feature Flags:");
            if (config.FeatureFlags != null)
            {
                foreach (var kvp in config.FeatureFlags)
                {
                    Console.WriteLine($" - {kvp.Key}: {kvp.Value} (Enabled: {config.IsFeatureEnabled(kvp.Key)})");
                }
            }
            else
            {
                Console.WriteLine(" - No feature flags defined.");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            Console.ResetColor();
        }
    }
}
```

**Output:**

```
Loading configuration...
Application Name: MyAwesomeService
Server Port: 9001
Feature Flags:
 - NewUI: true (Enabled: True)
 - DetailedLogging: false (Enabled: False)
 - UseBetaAPI: true (Enabled: True)
```

## Saving Configuration

You can also save configuration objects back to `.env` files:

```csharp
// Save current configuration values
EnvConfig.Save(config, "backup.env");

// Generate a template with default values
EnvConfig.SaveDefaults<AppConfig>("template.env");
```

This is useful for creating configuration templates, backing up settings, or generating example files for new team members.
