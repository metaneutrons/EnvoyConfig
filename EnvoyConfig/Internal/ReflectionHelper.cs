using System.Collections.Concurrent;
using System.Reflection;
using EnvoyConfig.Attributes;
using EnvoyConfig.Conversion; // Added for ITypeConverter and TypeConverterRegistry
using EnvoyConfig.Logging;

namespace EnvoyConfig.Internal;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles reflection, attribute discovery, and instance population for EnvConfig.
/// </summary>
public static class ReflectionHelper
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
                var currentPrefix = globalPrefix ?? string.Empty;

                // List (comma-separated)
                if (attr.IsList && !string.IsNullOrEmpty(attr.Key))
                {
                    value = HandleCommaSeparatedListProperty(prop, attr, logger, currentPrefix, variables);
                }
                // Direct Key (ensure this is not a list, as IsList is handled above)
                else if (!string.IsNullOrEmpty(attr.Key))
                {
                    value = HandleDirectKeyProperty(prop, attr, logger, currentPrefix, variables);
                }
                // Numbered List
                else if (!string.IsNullOrEmpty(attr.ListPrefix))
                {
                    value = HandleNumberedListProperty(prop, attr, logger, currentPrefix);
                }
                // Dictionary/Map
                else if (!string.IsNullOrEmpty(attr.MapPrefix))
                {
                    value = HandleMapProperty(prop, attr, logger, currentPrefix);
                }
                // Nested object with prefix
                else if (!string.IsNullOrEmpty(attr.NestedPrefix))
                {
                    value = HandleNestedObjectProperty(prop, attr, logger, currentPrefix);
                }
                // List of nested objects (NestedListPrefix/NestedListSuffix)
                else if (!string.IsNullOrEmpty(attr.NestedListPrefix) && !string.IsNullOrEmpty(attr.NestedListSuffix))
                {
                    value = HandleNestedListProperty(prop, attr, logger, currentPrefix);
                }

                if (value != null)
                {
                    prop.SetValue(instance, value);
                }
            }
            catch (Exception ex)
            {
                logger?.Log(EnvLogLevel.Error, $"Failed to load property '{prop.Name}': {ex.Message}");

                // If ThrowOnConversionError is true and this is a conversion-related exception, re-throw it
                if (EnvConfig.ThrowOnConversionError && ex is InvalidOperationException)
                {
                    throw;
                }
            }
        }
        return instance;
    }

    private static object? ConvertToType(string? str, Type type, IEnvLogSink? logger, string envKey)
    {
        try
        {
            // Check for custom converter first
            if (TypeConverterRegistry.TryGetConverter(type, out var customConverter) && customConverter != null)
            {
                // Note: The current ITypeConverter interface doesn't take envKey.
                // If specific error reporting with envKey is needed for custom converters,
                // the interface might need to be adjusted, or converters would need to handle logging internally.
                return customConverter.Convert(str, type, logger);
            }

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
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to int";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
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
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to bool";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
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

                if (!double.TryParse(str, out var d))
                {
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to double";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
                    return 0.0;
                }
                return d;
            }

            if (type == typeof(float))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0f;
                }

                if (!float.TryParse(str, out var f))
                {
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to float";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
                    return 0f;
                }
                return f;
            }

            if (type == typeof(long))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0L;
                }

                if (!long.TryParse(str, out var l))
                {
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to long";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
                    return 0L;
                }
                return l;
            }

            if (type == typeof(DateTime))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return default(DateTime);
                }

                if (!DateTime.TryParse(str, out var dt))
                {
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to DateTime";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
                    return default(DateTime);
                }
                return dt;
            }

            if (type == typeof(TimeSpan))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return default(TimeSpan);
                }

                if (!TimeSpan.TryParse(str, out var ts))
                {
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to TimeSpan";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
                    return default(TimeSpan);
                }
                return ts;
            }

            if (type == typeof(Guid))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return default(Guid);
                }

                if (!Guid.TryParse(str, out var guid))
                {
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to Guid";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
                    return default(Guid);
                }
                return guid;
            }

            if (type.IsEnum && !string.IsNullOrEmpty(str))
            {
                if (!Enum.TryParse(type, str, true, out var e))
                {
                    var errorMsg = $"Failed to convert '{envKey}' value '{str}' to enum type {type.Name}";
                    logger?.Log(EnvLogLevel.Error, errorMsg);
                    if (EnvConfig.ThrowOnConversionError)
                    {
                        throw new InvalidOperationException(errorMsg);
                    }
                    return Activator.CreateInstance(type);
                }
                return e;
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
            var errorMsg = $"Failed to convert '{envKey}' value '{str}' to {type.Name}: {ex.Message}";
            logger?.Log(EnvLogLevel.Error, errorMsg);
            if (EnvConfig.ThrowOnConversionError)
            {
                if (ex is InvalidOperationException)
                {
                    throw; // Re-throw the original exception
                }
                else
                {
                    throw new InvalidOperationException(errorMsg, ex); // Wrap other exceptions
                }
            }
        }
        // If ThrowOnConversionError is true, we should have already thrown.
        // If false, or if it's a non-conversion related exception that was caught by the generic catch,
        // return default value.
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private static object? HandleDirectKeyProperty(
        PropertyInfo prop,
        EnvAttribute attr,
        IEnvLogSink? logger,
        string globalPrefix,
        Dictionary<string, string>? variables
    )
    {
        var envKey = globalPrefix + attr.Key;
        var str = Environment.GetEnvironmentVariable(envKey);

        if ((str == null || str == "") && attr.Default != null)
        {
            var defaultObj = attr.Default;
            if (defaultObj is string s)
            {
                str = s;
                if (variables != null && str != null)
                {
                    foreach (var kv in variables)
                    {
                        str = str.Replace($"{{{kv.Key}}}", kv.Value);
                    }
                }
                return ConvertToType(str, prop.PropertyType, logger, envKey);
            }
            else
            {
                if (defaultObj != null && prop.PropertyType.IsInstanceOfType(defaultObj))
                {
                    return defaultObj;
                }
                else
                {
                    return ConvertToType(defaultObj?.ToString(), prop.PropertyType, logger, envKey);
                }
            }
        }
        else
        {
            return ConvertToType(str, prop.PropertyType, logger, envKey);
        }
    }

    private static object? HandleCommaSeparatedListProperty(
        PropertyInfo prop,
        EnvAttribute attr,
        IEnvLogSink? logger,
        string globalPrefix,
        Dictionary<string, string>? variables
    )
    {
        var envKey = globalPrefix + attr.Key;
        var str = Environment.GetEnvironmentVariable(envKey);

        if (string.IsNullOrEmpty(str) && attr.Default != null)
        {
            str = attr.Default is string s ? s : attr.Default.ToString();
            if (variables != null && str != null)
            {
                foreach (var kv in variables)
                {
                    str = str.Replace($"{{{kv.Key}}}", kv.Value);
                }
            }
        }
        return ParseList(str, prop.PropertyType, attr.ListSeparator, logger, envKey);
    }

    private static object? HandleNumberedListProperty(
        PropertyInfo prop,
        EnvAttribute attr,
        IEnvLogSink? logger,
        string globalPrefix
    )
    {
        return ParseNumberedList(globalPrefix + attr.ListPrefix, prop.PropertyType, logger);
    }

    private static object? HandleMapProperty(
        PropertyInfo prop,
        EnvAttribute attr,
        IEnvLogSink? logger,
        string globalPrefix
    )
    {
        return ParseMap(globalPrefix + attr.MapPrefix, prop.PropertyType, logger, attr.MapKeyCasing);
    }

    private static object? HandleNestedObjectProperty(
        PropertyInfo prop,
        EnvAttribute attr,
        IEnvLogSink? logger,
        string globalPrefix
    )
    {
        var nestedType = prop.PropertyType;
        return typeof(ReflectionHelper)
            .GetMethod(nameof(PopulateInstance), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(nestedType)
            .Invoke(null, new object?[] { logger, globalPrefix + attr.NestedPrefix, null });
    }

    private static object? HandleNestedListProperty(
        PropertyInfo prop,
        EnvAttribute attr,
        IEnvLogSink? logger,
        string globalPrefix
    )
    {
        return ParseNestedList(globalPrefix, attr, prop.PropertyType, logger);
    }

    // Original parsing methods (ParseList, ParseNumberedList, ParseMap, ParseNestedList) remain unchanged below this line
    // but are now called by the new Handle* methods.

    internal static object? ParseList(string? str, Type type, char sep, IEnvLogSink? logger, string envKey) // Made internal for potential testing, was private
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

    internal static object? ParseNumberedList(string prefix, Type type, IEnvLogSink? logger) // Made internal for potential testing, was private
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

    internal static object? ParseMap( // Made internal for potential testing, was private
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
    internal static object? ParseNestedList(string globalPrefix, EnvAttribute attr, Type listType, IEnvLogSink? logger) // Made internal for potential testing, was private
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
                .GetMethod(nameof(PopulateInstance), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(elemType)
                .Invoke(null, [logger, nestedPrefix, variables]);
            list.Add(nested);
        }
        return list;
    }
}
