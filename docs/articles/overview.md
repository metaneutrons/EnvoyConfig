# Overview & Features

## Key Features & Design Goals

* **Attribute-First Mapping:** Use declarative `[Env]` attributes directly on your C# properties to define the mapping from environment variables. This keeps configuration logic close to the data structures it populates, enhancing readability and maintainability.
* **Versatile Parsing Strategies:** Supports multiple common patterns for structuring environment variables:
  * Simple key-to-property mapping.
  * Parsing structured key-value pairs within a single variable.
  * Mapping prefixed groups of variables to nested objects.
  * Handling lists of simple types (comma-separated).
  * Handling lists of complex objects (indexed prefixes).
* **Zero External Dependencies:** EC has no runtime dependencies on other libraries, including `Microsoft.Extensions.Configuration` or JSON parsing libraries. This makes it ideal for:
  * Minimal API projects.
  * Azure Functions or AWS Lambda where cold starts and package size matter.
  * Containerized applications aiming for small image sizes.
  * Libraries that shouldn't impose external configuration dependencies.
* **Testability:** The library's static nature and reliance on standard POCOs make it straightforward to unit test your configuration loading logic. You can easily mock environment variables or use dedicated `.env` files during testing.
* **Type Safety:** Binds environment variable strings to appropriate .NET types (integers, booleans, lists, dictionaries, etc.), catching potential errors early.
* **Bidirectional Operations:** Not only load configuration from environment variables, but also save configuration objects back to `.env` files and generate template files with default values.

## When to Use EnvoyConfig (and When Not To)

**Choose EnvoyConfig if:**

* Your application primarily uses environment variables for configuration.
* You prefer declarative, attribute-based mapping.
* You need a zero-dependency configuration solution.
* You value simplicity for environment-specific configuration.
* You are building minimal APIs, serverless functions, or small containerized services.

**Consider alternatives (like `Microsoft.Extensions.Configuration`) if:**

* You need to load configuration from multiple sources simultaneously (e.g., JSON files, Azure Key Vault, command-line arguments) and require sophisticated layering/overriding rules.
* You are deeply integrated with the ASP.NET Core ecosystem and rely on its built-in dependency injection for configuration (`IOptions<T>`).
* You require complex configuration transformations or post-processing logic beyond simple type binding.
