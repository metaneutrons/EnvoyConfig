# Core Concepts

## Parsing Strategies

EnvoyConfig uses the `[Env]` attribute on properties within your configuration classes to determine how to map environment variables. The behavior is controlled by the attribute's parameters (`Name`, `DefaultValue`, `IsList`, `IsKeyValue`, `Prefix`).

Here are the five primary mapping strategies:

| Strategy     | Description                                                                  | Attribute Usage Example                        | Environment Variable Example(s)             | Target Property Type         |
| :----------- | :--------------------------------------------------------------------------- | :--------------------------------------------- | :------------------------------------------ | :--------------------------- |
| DirectKey    | Maps a single environment variable directly to a single property.            | `[Env("VAR_NAME")]`                            | `VAR_NAME=value`                            | `string`, `int`, `bool`, etc. |
| List         | Parses a single comma-separated environment variable into a list.            | `[Env("LIST_VAR", IsList = true)]`             | `LIST_VAR=item1,item2,item3`                | `List<string>`, `List<int>`, etc. |
| KeyValue     | Parses a single variable containing `key=value` pairs (semicolon-separated) into a dictionary. | `[Env("DICT_VAR", IsKeyValue = true)]`         | `DICT_VAR=key1=val1;key2=val2`              | `Dictionary<string, string>`¹ |
| Nested       | Maps a group of environment variables sharing a common prefix to properties of a nested class instance. | `[Env(Prefix = "PREFIX_")]` (on property holding nested object) | `PREFIX_PROP1=val1`<br/>`PREFIX_PROP2=val2` | Your custom nested class     |
| NestedList   | Maps multiple groups of prefixed environment variables (using an index) to a list of nested class instances. | `[Env(Prefix = "ITEM_", IsList = true)]` (on property holding list) | `ITEM_0_NAME=A`<br/>`ITEM_0_URL=url_a`<br/>`ITEM_1_NAME=B`<br/>`ITEM_1_URL=url_b` | `List<YourCustomClass>`      |

¹ *Currently, KeyValue primarily targets `Dictionary<string, string>`. Support for `Dictionary<string, T>` might require parsing the value string.*

### The `[Env]` Attribute Parameters

* **`Name` (string):**
  * For `DirectKey`, `List`, `KeyValue`: Specifies the exact name of the environment variable to read.
  * *Not used* directly for `Nested` or `NestedList` strategies (the `Prefix` parameter is used instead).
* **`Prefix` (string):**
  * For `Nested`: Specifies the prefix shared by environment variables that map to the nested object's properties. EC will look for variables named `{Prefix}{PropertyName}`.
  * For `NestedList`: Specifies the prefix used for identifying list items and their properties. EC will look for variables named `{Prefix}{Index}_{PropertyName}`.
* **`DefaultValue` (object):**
  * Provides a fallback value if the corresponding environment variable is not found or is empty.
  * The type of the default value should be compatible with the target property type. EnvoyConfig will attempt type conversion.
  * **Crucial for required settings:** If a variable is essential but might not be set, throwing an exception *after* loading (validation) is often better than providing a potentially unsafe default. However, for optional settings or development defaults, this is very useful.
* **`IsList` (bool):**
  * Default: `false`.
  * Set to `true` to enable list parsing.
  * If `Prefix` is **not** set, enables the `List` strategy (comma-separated values). The `Name` parameter specifies the variable name.
  * If `Prefix` **is** set, enables the `NestedList` strategy (indexed prefixes). The `Prefix` parameter specifies the base prefix.
* **`IsKeyValue` (bool):**
  * Default: `false`.
  * Set to `true` to enable the `KeyValue` strategy. Parses a string like `key1=value1;key2=value2` into a `Dictionary<string, string>`. The `Name` parameter specifies the variable name.
