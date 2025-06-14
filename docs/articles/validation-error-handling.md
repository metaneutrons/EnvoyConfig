# Validation & Error Handling

EnvoyConfig focuses on *loading* and *binding*. It provides basic checks via `DefaultValue`, but robust validation should generally occur *after* loading.

* **Missing Variables:**
  * If an environment variable specified by `[Env("NAME")]` or required by a `Prefix` is missing **and** no `DefaultValue` is provided, the property will remain at its default C# value (e.g., `null` for reference types, `0` for `int`).
  * **Recommendation:** For required settings without sensible defaults, check for `null` or invalid values after calling `Load<T>()` and throw a custom configuration exception.
* **Type Conversion Errors:**
  EnvoyConfig provides flexibility in how type conversion errors are handled through the `EnvConfig.ThrowOnConversionError` static property.
    *   **Default Behavior (`EnvConfig.ThrowOnConversionError = false`):**
        If an environment variable's string value cannot be converted to the target property type (e.g., "text" for an `int` property), EnvoyConfig will:
        1.  Log an error message (if an `IEnvLogSink` is provided to `EnvConfig.Load<T>()`).
        2.  Assign the default C# value to the property (e.g., `0` for `int`, `null` for reference types, `DateTime.MinValue` for `DateTime`).
        The loading process continues for other properties. This is useful if you prefer your application to start with default values for misconfigured items rather than failing to start entirely. You would then typically check logs or property values to determine if critical configurations were defaulted.

    *   **Strict Error Handling (`EnvConfig.ThrowOnConversionError = true`):**
        If you set `EnvConfig.ThrowOnConversionError = true;` before calling `EnvConfig.Load<T>()`, any failure to convert an environment variable to its target property type will immediately throw an `InvalidOperationException`. This exception will contain details about the environment variable key, the problematic value, and the target type. This approach ensures that the application fails fast if configurations are incorrect, which can be preferable in environments where configuration errors should halt deployment or startup.

        ```csharp
        // Example: Enabling strict error handling
        EnvConfig.ThrowOnConversionError = true;
        try
        {
            var config = EnvConfig.Load<MyApplicationConfig>();
            // Proceed if loading is successful
        }
        catch (InvalidOperationException ex)
        {
            // Log the critical error and terminate or handle as appropriate
            Console.WriteLine($"FATAL: Configuration loading failed due to a type conversion error: {ex.Message}");
            // Potentially rethrow or Environment.Exit(1);
        }
        ```
    Regardless of the `ThrowOnConversionError` setting, it's good practice to implement post-loading validation for business rules that go beyond simple type checks.

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
