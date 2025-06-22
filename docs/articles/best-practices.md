# Best Practices

1. **Clear Defaults:** Use `DefaultValue` for settings that have sensible, non-critical defaults (like ports in development) or are truly optional. Avoid defaults for secrets or essential parameters where absence indicates an error.
2. **Validate Early & Explicitly:** Don't rely solely on defaults. Add explicit validation logic after `Load<T>()` to ensure required settings are present and valid. Fail fast if critical configuration is missing.
3. **Document Your Environment Schema:** Maintain an `.env.example` or `.env.template` file in your repository. This file should list all potential environment variables your application uses, with descriptions and example (but non-secret) values.
4. **Secure Secrets:** **Never** commit `.env` files containing sensitive information (API keys, passwords) to source control. Use `.gitignore` to exclude them. Rely on secure environment variable injection from your CI/CD system, PaaS provider (like Azure App Service Configuration, AWS Parameter Store), or container orchestrator (Kubernetes Secrets).
5. **Prefix for Clarity:** Use prefixes (`DB_`, `CACHE_`, `AWS_`) consistently for related variables, especially in larger applications, even if not using the `Nested` strategy. This avoids naming collisions.
6. **Test Configuration Loading:**
    * Create test-specific `.env` files and use `EnvConfig.UseDotEnv("path/to/test.env")` in your test setup.
    * Alternatively, directly set environment variables in your test runner's setup/teardown phases: `Environment.SetEnvironmentVariable("TEST_VAR", "value");` ... `Environment.SetEnvironmentVariable("TEST_VAR", null);`.
    * Verify that defaults are applied correctly and that missing required variables are handled (either by your validation or expected `null`/default values).
7. **Use Save Functionality for Templates & Documentation:**
    * Generate `.env.example` files using `EnvConfig.SaveDefaults<T>()` to provide clear examples of required environment variables.
    * Use `EnvConfig.Save()` to export working configurations for backup, sharing between environments, or creating deployment templates.
    * Regenerate example files when you add new configuration properties to keep documentation current.
8. **Configuration Management Workflow:**
    * **Development**: Use `.env` files for local development with `EnvConfig.UseDotEnv()`.
    * **Testing**: Generate test configurations with `EnvConfig.SaveDefaults<T>()` and customize as needed.
    * **Production**: Use actual environment variables (container orchestrators, CI/CD systems) but validate with saved templates.
    * **Documentation**: Keep `.env.example` files updated using `SaveDefaults<T>()` for onboarding new developers.

## Contributing & Support (Optional)

* Link to your source code repository (GitHub, etc.).
* Mention how to report bugs or suggest features (e.g., GitHub Issues).
* Provide contribution guidelines if applicable.
