using System.Collections.Concurrent;
using System.Reflection;
using EnvoyConfig.Attributes;
using EnvoyConfig.Logging;

namespace EnvoyConfig.Internal;

/// <summary>
/// Handles reflection, attribute discovery, and instance population.
/// </summary>
/// <summary>
/// Handles reflection, attribute discovery, and instance population for EnvConfig.
/// </summary>
internal static class ReflectionHelper
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
    private static readonly ConcurrentDictionary<Type, List<(PropertyInfo, EnvAttribute)>> _envPropCache = new();

    /// <summary>
    /// Populates an instance of type T from environment variables, using EnvAttribute metadata.
    /// </summary>
    /// <typeparam name="T">The configuration class type to populate.</typeparam>
    /// <param name="logger">Optional logger for warnings and errors.</param>
    /// <param name="globalPrefix">Optional global prefix for all lookups.</param>
    /// <param name="variables">Optional dictionary of variables to use instead of environment variables.</param>
    /// <returns>The populated configuration object.</returns>
    public static T PopulateInstance<T>(
        IEnvLogSink? logger = null,
        string? globalPrefix = null,
        Dictionary<string, string>? variables = null
    )
        where T : new()
    {
        var type = typeof(T);
        var props = _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        var envProps = _envPropCache.GetOrAdd(
            type,
            t =>
                props
                    .Select(p => (p, attr: p.GetCustomAttribute<EnvAttribute>()))
                    .Where(x => x.attr != null)
                    .Select(x => (x.p, x.attr!))
                    .ToList()
        );

        var instance = new T();
        foreach (var (prop, attr) in envProps)
        {
            try
            {
                object? value = null;
                var prefix = globalPrefix ?? string.Empty;
                // List (comma-separated)
                if (attr.IsList && !string.IsNullOrEmpty(attr.Key))
                {
                    var envKey = prefix + attr.Key;
                    var str = Environment.GetEnvironmentVariable(envKey);
                    if (string.IsNullOrEmpty(str) && attr.Default != null)
                    {
                        str = attr.Default;
                        if (variables != null && str != null)
                        {
                            foreach (var kv in variables)
                            {
                                str = str.Replace($"{{{kv.Key}}}", kv.Value);
                            }
                        }
                    }
                    value = ParseList(str, prop.PropertyType, attr.ListSeparator, logger, envKey);
                }
                // Direct Key
                else if (!string.IsNullOrEmpty(attr.Key))
                {
                    var envKey = prefix + attr.Key;
                    var str = Environment.GetEnvironmentVariable(envKey);
                    if ((str == null || str == "") && attr.Default != null)
                    {
                        str = attr.Default;
                        if (variables != null && str != null)
                        {
                            foreach (var kv in variables)
                            {
                                str = str.Replace($"{{{kv.Key}}}", kv.Value);
                            }
                        }
                    }

                    value = ConvertToType(str, prop.PropertyType, logger, envKey);
                }
                // Numbered List
                else if (!string.IsNullOrEmpty(attr.ListPrefix))
                {
                    value = ParseNumberedList(prefix + attr.ListPrefix, prop.PropertyType, logger);
                }
                // Dictionary/Map
                else if (!string.IsNullOrEmpty(attr.MapPrefix))
                {
                    value = ParseMap(prefix + attr.MapPrefix, prop.PropertyType, logger, attr.MapKeyCasing);
                }
                // Nested object with prefix
                else if (!string.IsNullOrEmpty(attr.NestedPrefix))
                {
                    var nestedType = prop.PropertyType;
                    var nestedInstance = typeof(ReflectionHelper)
                        .GetMethod(
                            nameof(PopulateInstance),
                            BindingFlags.Public | BindingFlags.Static
                        )!
                        .MakeGenericMethod(nestedType)
                        .Invoke(null, new object?[] { logger, prefix + attr.NestedPrefix, null });
                    value = nestedInstance;
                }
                // List of nested objects (NestedListPrefix/NestedListSuffix)
                else if (!string.IsNullOrEmpty(attr.NestedListPrefix) && !string.IsNullOrEmpty(attr.NestedListSuffix))
                {
                    value = ParseNestedList(prefix, attr, prop.PropertyType, logger);
                }
                if (value != null)
                {
                    prop.SetValue(instance, value);
                }
            }
            catch (Exception ex)
            {
                logger?.Log(EnvLogLevel.Error, $"Failed to load property '{prop.Name}': {ex.Message}");
            }
        }
        return instance;
    }

    private static object? ConvertToType(string? str, Type type, IEnvLogSink? logger, string envKey)
    {
        try
        {
            if (type == typeof(string))
            {
                return str;
            }

            if (type == typeof(int))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0;
                }

                if (!int.TryParse(str, out var i))
                {
                    logger?.Log(EnvLogLevel.Error, $"Failed to convert '{envKey}' value '{str}' to int");
                    return 0;
                }
                return i;
            }

            if (type == typeof(bool))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return false;
                }

                if (!bool.TryParse(str, out var b))
                {
                    logger?.Log(EnvLogLevel.Error, $"Failed to convert '{envKey}' value '{str}' to bool");
                    return false;
                }
                return b;
            }

            if (type == typeof(double))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0.0;
                }

                return double.TryParse(str, out var d) ? d : 0.0;
            }

            if (type == typeof(float))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0f;
                }

                return float.TryParse(str, out var f) ? f : 0f;
            }

            if (type == typeof(long))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0L;
                }

                return long.TryParse(str, out var l) ? l : 0L;
            }

            if (type.IsEnum && !string.IsNullOrEmpty(str))
            {
                return Enum.TryParse(type, str, true, out var e) ? e : Activator.CreateInstance(type);
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }

                return ConvertToType(str, Nullable.GetUnderlyingType(type)!, logger, envKey);
            }
            // Add more types as needed
        }
        catch (Exception ex)
        {
            logger?.Log(EnvLogLevel.Error, $"Failed to convert '{envKey}' value '{str}' to {type.Name}: {ex.Message}");
        }
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private static object? ParseList(string? str, Type type, char sep, IEnvLogSink? logger, string envKey)
    {
        if (string.IsNullOrEmpty(str))
        {
            // Return an empty list/array instead of null instance for lists
            if (type.IsArray)
            {
                return Array.CreateInstance(type.GetElementType()!, 0);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return Activator.CreateInstance(type);
            }

            return Activator.CreateInstance(type);
        }

        var elemType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments.FirstOrDefault();
        if (elemType == null)
        {
            throw new InvalidOperationException("Element type cannot be determined.");
        }

        var items = str.Split(sep).Select(s => ConvertToType(s.Trim(), elemType, logger, envKey)).ToList();
        if (type.IsArray)
        {
            var arr = Array.CreateInstance(elemType, items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                arr.SetValue(items[i], i);
            }

            return arr;
        }
        var listType = typeof(List<>).MakeGenericType(elemType);
        var list = Activator.CreateInstance(listType) as System.Collections.IList;
        foreach (var item in items)
        {
            list!.Add(item);
        }

        return list;
    }

    private static object? ParseNumberedList(string prefix, Type type, IEnvLogSink? logger)
    {
        var elemType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments.FirstOrDefault();
        if (elemType == null)
        {
            throw new InvalidOperationException("Element type cannot be determined.");
        }

        var items = new List<object?>();
        for (var i = 1; ; i++)
        {
            var key = prefix + i;
            var str = Environment.GetEnvironmentVariable(key);
            if (str == null)
            {
                break;
            }

            items.Add(ConvertToType(str, elemType, logger, key));
        }
        if (type.IsArray)
        {
            var arr = Array.CreateInstance(elemType, items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                arr.SetValue(items[i], i);
            }

            return arr;
        }
        var listType = typeof(List<>).MakeGenericType(elemType);
        var list = Activator.CreateInstance(listType) as System.Collections.IList;
        foreach (var item in items)
        {
            list!.Add(item);
        }

        return list;
    }

    private static object? ParseMap(
        string prefix,
        Type type,
        IEnvLogSink? logger,
        MapKeyCasingMode casingMode = MapKeyCasingMode.Lower
    )
    {
        var args = type.GenericTypeArguments;
        if (args.Length != 2)
        {
            throw new InvalidOperationException("Type must have exactly two generic arguments.");
        }

        var dictType = typeof(Dictionary<,>).MakeGenericType(args);
        var dict = Activator.CreateInstance(dictType) as System.Collections.IDictionary;
        foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            var key = env.Key as string;
            if (key == null || !key.StartsWith(prefix))
            {
                continue;
            }

            var dictKey = key.Substring(prefix.Length);
            switch (casingMode)
            {
                case MapKeyCasingMode.Lower:
                    dictKey = dictKey.ToLowerInvariant();
                    break;
                case MapKeyCasingMode.Upper:
                    dictKey = dictKey.ToUpperInvariant();
                    break;
                case MapKeyCasingMode.AsIs:
                default:
                    break;
            }
            var dictKeyObj = ConvertToType(dictKey, args[0], logger, key);
            if (dictKeyObj == null)
            {
                throw new InvalidOperationException("Dictionary key cannot be null.");
            }

            var val = ConvertToType(env.Value?.ToString(), args[1], logger, key);
            dict!.Add(dictKeyObj, val);
        }
        return dict;
    }

    // Helper for parsing a list of nested objects from env vars with numbered prefixes
    private static object? ParseNestedList(string globalPrefix, EnvAttribute attr, Type listType, IEnvLogSink? logger)
    {
        // Only works for List<T>
        if (!listType.IsGenericType || listType.GetGenericTypeDefinition() != typeof(List<>))
        {
            throw new InvalidOperationException("NestedListPrefix only supported for List<T>");
        }

        var elemType = listType.GenericTypeArguments[0];
        var envVars = Environment.GetEnvironmentVariables();
        var prefix = globalPrefix + (attr.NestedListPrefix ?? "");
        var suffix = attr.NestedListSuffix ?? "";
        // Find all keys like SNAPDOG_ZONE_1_MQTT_*
        var indices = new HashSet<string>();
        foreach (System.Collections.DictionaryEntry e in envVars)
        {
            if (e.Key is string k && k.StartsWith(prefix) && k.Contains(suffix))
            {
                var rest = k.Substring(prefix.Length);
                var idxEnd = rest.IndexOf(suffix);
                if (idxEnd > 0)
                {
                    var idx = rest.Substring(0, idxEnd);
                    indices.Add(idx);
                }
            }
        }
        var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType))!;
        foreach (var idx in indices.OrderBy(i => i))
        {
            // Compose prefix for this nested object
            var nestedPrefix = prefix + idx + suffix;
            var variables = new Dictionary<string, string> { ["index"] = idx };
            var nested = typeof(ReflectionHelper)
                .GetMethod(
                    nameof(PopulateInstance),
                    BindingFlags.Public | BindingFlags.Static
                )!
                .MakeGenericMethod(elemType)
                .Invoke(null, new object?[] { logger, nestedPrefix, variables });
            list.Add(nested);
        }
        return list;
    }
}
