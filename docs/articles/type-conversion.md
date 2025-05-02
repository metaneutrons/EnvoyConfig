# Type Conversion

EnvoyConfig attempts to convert the string values from environment variables to the target property types. Supported conversions typically include:

* `string` (no conversion needed)
* `int`, `long`, `short`, `byte`
* `uint`, `ulong`, `ushort`, `sbyte`
* `float`, `double`, `decimal`
* `bool` (case-insensitive "true", "1" usually map to `true`; others to `false`)
* `char` (first character of the string)
* `DateTime`, `DateTimeOffset`, `TimeSpan` (using standard parsing rules)
* `Uri`
* `Guid`
* `Enum` types (case-insensitive string matching to enum value names)
* `List<T>` (where T is one of the above simple types, for the `List` strategy)
* `Dictionary<string, string>` (for the `KeyValue` strategy)

**Error Handling:** If a conversion fails (e.g., trying to parse "abc" as an `int`), `EnvConfig.Load<T>()` will typically throw a `FormatException` or `InvalidCastException`. Ensure you have appropriate error handling around the `Load` call.
