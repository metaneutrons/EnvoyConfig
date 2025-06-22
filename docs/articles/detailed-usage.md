# Detailed Usage

## Loading Environment Sources

EnvoyConfig primarily reads from `System.Environment.GetEnvironmentVariable`.

* **System Environment Variables:** These are read by default. Variables set at the OS level, via Docker `--env` flags, Kubernetes manifests, or CI/CD pipelines are automatically available.
* **`.env` File Integration:**
  * Call `EnvConfig.UseDotEnv()` **before** `EnvConfig.Load<T>()`.
  * This method reads the specified `.env` file (defaults to `.env` in the current directory) and loads its key-value pairs into the process's environment variables using `Environment.SetEnvironmentVariable`.
  * `UseDotEnv` will **not** override environment variables that are *already set* in the process's environment. System-level variables take precedence over `.env` file variables.
  * You can specify a custom path: `EnvConfig.UseDotEnv("path/to/your/.env")`.

## Defining Configuration Classes - Examples

### DirectKey

```csharp
public class ApiClientConfig
{
    [Env("SERVICE_URL")]
    public Uri ServiceUrl { get; set; }
    [Env("TIMEOUT_SECONDS", DefaultValue = 15)]
    public int TimeoutSeconds { get; set; }
    [Env("ENABLE_CACHING", DefaultValue = false)]
    public bool EnableCaching { get; set; }
    [Env("LOG_LEVEL", DefaultValue = LogLevel.Warning)]
    public LogLevel MinimumLogLevel { get; set; }
}
```

### List (Comma-Separated)

```csharp
public class CorsConfig
{
    [Env("ALLOWED_ORIGINS", IsList = true)]
    public List<string> AllowedOrigins { get; set; }
    [Env("PORT_NUMBERS", IsList = true)]
    public List<int> AllowedPorts { get; set; }
}
```

### KeyValue (Dictionary)

```csharp
public class DatabaseConnection
{
    [Env("CONNECTION_SETTINGS", IsKeyValue = true)]
    public Dictionary<string, string> Settings { get; set; }
    public string GetConnectionString()
    {
        if (Settings == null) return string.Empty;
        return string.Join(";", Settings.Select(kv => $"{kv.Key}={kv.Value}"));
    }
}
```

### Nested Objects

```csharp
public class FullAppConfig
{
    [Env(Prefix = "DB_")]
    public DatabaseConfig Database { get; set; }
    [Env(Prefix = "CACHE_")]
    public CacheConfig Cache { get; set; }
}
public class DatabaseConfig
{
    [Env("HOST")]
    public string Host { get; set; }
    [Env("PORT", DefaultValue = 5432)]
    public int Port { get; set; }
    [Env("USERNAME")]
    public string Username { get; set; }
    [Env("PASSWORD")]
    public string Password { get; set; }
}
public class CacheConfig
{
    [Env("HOST", DefaultValue = "localhost")]
    public string Host { get; set; }
    [Env("PORT", DefaultValue = 6379)]
    public int Port { get; set; }
    [Env("DB_INDEX", DefaultValue = 0)]
    public int DbIndex { get; set; }
}
```

### Nested List (List of Objects)

```csharp
public class ServiceRegistryConfig
{
    [Env(Prefix = "SERVICE_", IsList = true)]
    public List<ServiceEndpoint> Services { get; set; }
}
public class ServiceEndpoint
{
    [Env("NAME")]
    public string Name { get; set; }
    [Env("URL")]
    public string Url { get; set; }
    [Env("TIMEOUT", DefaultValue = 10)]
    public int TimeoutSeconds { get; set; }
}
```

## Loading Configuration at Runtime

```csharp
EnvConfig.UseDotEnv();
ServerConfig serverSettings = EnvConfig.Load<ServerConfig>();
CorsConfig corsSettings = EnvConfig.Load<CorsConfig>();
FullAppConfig appConfiguration = EnvConfig.Load<FullAppConfig>();
Console.WriteLine($"Database Host: {appConfiguration.Database?.Host}");
Console.WriteLine($"Number of services defined: {appConfiguration.Services?.Count}");
```

## Saving Configuration to Files

EnvoyConfig also supports saving configuration objects back to `.env` files, making it easy to:

* **Export current settings**: Save the current state of a configuration object to a `.env` file.
* **Generate templates**: Create `.env` template files with default values for easy setup.
* **Configuration backup**: Create backups of working configurations.

### Saving Current Configuration Values

Use [`EnvConfig.Save()`](EnvConfig.cs:1) to save the current values of a configuration object:

```csharp
public class ServerConfig
{
    [Env("SERVER_PORT", DefaultValue = 8080)]
    public int Port { get; set; } = 9001;

    [Env("SERVER_HOST", DefaultValue = "localhost")]
    public string Host { get; set; } = "api.example.com";

    [Env("FEATURE_FLAGS", IsKeyValue = true)]
    public Dictionary<string, string> Features { get; set; } = new()
    {
        ["logging"] = "true",
        ["metrics"] = "false"
    };
}

// Save current configuration values
var config = new ServerConfig();
EnvConfig.Save(config, "current-settings.env");
```

**Generated `current-settings.env`:**

```env
SERVER_PORT=9001
SERVER_HOST=api.example.com
FEATURE_FLAGS_logging=true
FEATURE_FLAGS_metrics=false
```

### Generating Default Value Templates

Use [`EnvConfig.SaveDefaults<T>()`](EnvConfig.cs:1) to generate template files with default values:

```csharp
// Generate a template with default values
EnvConfig.SaveDefaults<ServerConfig>("template.env");
```

**Generated `template.env`:**

```env
SERVER_PORT=8080
SERVER_HOST=localhost
FEATURE_FLAGS_=
```

### Save with Complex Configurations

The Save functionality supports all parsing strategies:

```csharp
public class ComplexConfig
{
    [Env("APP_NAME")]
    public string AppName { get; set; } = "MyApp";

    [Env("ALLOWED_IPS", IsList = true)]
    public List<string> AllowedIPs { get; set; } = new() { "127.0.0.1", "192.168.1.1" };

    [Env(Prefix = "NUMBERED_", IsList = true)]
    public List<string> NumberedItems { get; set; } = new() { "item1", "item2", "item3" };

    [Env(Prefix = "DB_")]
    public DatabaseConfig Database { get; set; } = new()
    {
        Host = "localhost",
        Port = 5432
    };

    [Env(Prefix = "SERVICE_", IsList = true)]
    public List<ServiceEndpoint> Services { get; set; } = new()
    {
        new() { Name = "auth", Url = "https://auth.api.com" },
        new() { Name = "data", Url = "https://data.api.com" }
    };
}

EnvConfig.Save(complexConfig, "complex.env");
```

**Generated `complex.env`:**

```env
APP_NAME=MyApp
ALLOWED_IPS=127.0.0.1,192.168.1.1
NUMBERED_1=item1
NUMBERED_2=item2
NUMBERED_3=item3
DB_HOST=localhost
DB_PORT=5432
SERVICE_1_NAME=auth
SERVICE_1_URL=https://auth.api.com
SERVICE_2_NAME=data
SERVICE_2_URL=https://data.api.com
```

## Global Prefix Usage

The [`EnvConfig.GlobalPrefix`](EnvConfig.cs:1) property allows you to add a common prefix to all environment variable names throughout your application. This is particularly useful for:

* **Multi-tenant applications**: Isolate configuration between different instances
* **Microservices**: Prevent naming collisions when multiple services share the same environment
* **Development environments**: Separate configuration between different projects or branches

### Setting Global Prefix

```csharp
// Set global prefix before any Load or Save operations
EnvConfig.GlobalPrefix = "MYAPP_";
```

### Loading with Global Prefix

When a global prefix is set, EnvoyConfig automatically prepends it to all environment variable lookups:

```csharp
public class DatabaseConfig
{
    [Env("DB_HOST")] // Maps to MYAPP_DB_HOST
    public string Host { get; set; }

    [Env("DB_PORT", DefaultValue = 5432)] // Maps to MYAPP_DB_PORT
    public int Port { get; set; }

    [Env("DB_NAME")] // Maps to MYAPP_DB_NAME
    public string DatabaseName { get; set; }
}

// Set global prefix
EnvConfig.GlobalPrefix = "MYAPP_";

// Load configuration - will look for MYAPP_DB_HOST, MYAPP_DB_PORT, MYAPP_DB_NAME
var dbConfig = EnvConfig.Load<DatabaseConfig>();
```

**Environment variables expected:**

```env
MYAPP_DB_HOST=localhost
MYAPP_DB_PORT=5432
MYAPP_DB_NAME=production_db
```

### Global Prefix with Different Strategies

Global prefix works with all parsing strategies:

```csharp
public class ComplexConfig
{
    // DirectKey: looks for MYAPP_APP_NAME
    [Env("APP_NAME")]
    public string AppName { get; set; }

    // List: looks for MYAPP_ALLOWED_IPS
    [Env("ALLOWED_IPS", IsList = true)]
    public List<string> AllowedIPs { get; set; }

    // KeyValue: looks for MYAPP_FEATURE_FLAGS
    [Env("FEATURE_FLAGS", IsKeyValue = true)]
    public Dictionary<string, string> Features { get; set; }

    // Nested: looks for MYAPP_DB_HOST, MYAPP_DB_PORT, etc.
    [Env(Prefix = "DB_")]
    public DatabaseConfig Database { get; set; }

    // NestedList: looks for MYAPP_SERVICE_1_NAME, MYAPP_SERVICE_1_URL, etc.
    [Env(Prefix = "SERVICE_", IsList = true)]
    public List<ServiceEndpoint> Services { get; set; }
}
```

### Saving with Global Prefix

Global prefix is also applied during save operations:

```csharp
var config = new DatabaseConfig
{
    Host = "prod-server.com",
    Port = 5432,
    DatabaseName = "production_db"
};

EnvConfig.GlobalPrefix = "MYAPP_";
EnvConfig.Save(config, "production.env");
```

**Generated `production.env`:**

```env
MYAPP_DB_HOST=prod-server.com
MYAPP_DB_PORT=5432
MYAPP_DB_NAME=production_db
```

### Global Prefix Best Practices

1. **Set Early**: Configure the global prefix before any configuration loading or saving operations.
2. **Consistent Naming**: Use descriptive prefixes that clearly identify your application or service.
3. **Environment Separation**: Use different prefixes for different environments:

   ```csharp
   // Development
   EnvConfig.GlobalPrefix = "DEV_MYAPP_";

   // Staging
   EnvConfig.GlobalPrefix = "STAGING_MYAPP_";

   // Production
   EnvConfig.GlobalPrefix = "PROD_MYAPP_";
   ```

4. **Reset When Needed**: Clear the global prefix when testing or switching contexts:

   ```csharp
   EnvConfig.GlobalPrefix = null; // or string.Empty
   ```

### Global Prefix with .env Files

When using `.env` files with global prefix, ensure your file contains the prefixed variable names:

**.env file:**

```env
MYAPP_SERVER_PORT=8080
MYAPP_SERVER_HOST=localhost
MYAPP_DB_HOST=localhost
MYAPP_DB_PORT=5432
MYAPP_FEATURE_FLAGS_logging=true
MYAPP_FEATURE_FLAGS_metrics=false
```

**Loading:**

```csharp
EnvConfig.GlobalPrefix = "MYAPP_";
EnvConfig.UseDotEnv();
var config = EnvConfig.Load<ServerConfig>();
```
