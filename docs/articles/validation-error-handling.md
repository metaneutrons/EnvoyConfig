# Validation & Error Handling

EnvoyConfig focuses on *loading* and *binding*. It provides basic checks via `DefaultValue`, but robust validation should generally occur *after* loading.

* **Missing Variables:**
  * If an environment variable specified by `[Env("NAME")]` or required by a `Prefix` is missing **and** no `DefaultValue` is provided, the property will remain at its default C# value (e.g., `null` for reference types, `0` for `int`).
  * **Recommendation:** For required settings without sensible defaults, check for `null` or invalid values after calling `Load<T>()` and throw a custom configuration exception.
* **Type Conversion Errors:** As mentioned, invalid formats will cause exceptions during `Load<T>()`. Use `try-catch` blocks.
* **Post-Loading Validation:** Leverage standard .NET validation techniques:
  * Add validation methods to your configuration classes (e.g., `Validate()` method).
  * Use libraries like `FluentValidation`.
  * Use Data Annotations (`[Required]`, `[Range]`, `[Url]`) and a validation runner *after* loading the POCO.

```csharp
public class CriticalConfig
{
    [Env("API_KEY")]
    public string ApiKey { get; set; }
    [Env("MAX_RETRIES", DefaultValue = 3)]
    public int MaxRetries { get; set; }
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new InvalidOperationException("Configuration Error: API_KEY is required.");
        }
        if (MaxRetries < 0 || MaxRetries > 10)
        {
            throw new InvalidOperationException("Configuration Error: MAX_RETRIES must be between 0 and 10.");
        }
    }
}

try
{
    var config = EnvConfig.Load<CriticalConfig>();
    config.Validate();
}
catch (Exception ex)
{
    Console.WriteLine($"Configuration failed: {ex.Message}");
}
```
