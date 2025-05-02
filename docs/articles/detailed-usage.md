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
