using System;
using System.Collections.Generic;

namespace EnvoyConfig.Conversion
{
    /// <summary>
    /// Manages custom type converters.
    /// </summary>
    public static class TypeConverterRegistry
    {
        private static readonly Dictionary<Type, ITypeConverter> _converters = new();

        /// <summary>
        /// Registers a custom converter for a specific type.
        /// </summary>
        /// <param name="type">The type for which to register the converter.</param>
        /// <param name="converter">The converter instance.</param>
        public static void RegisterConverter(Type type, ITypeConverter converter)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            _converters[type] = converter;
        }

        /// <summary>
        /// Attempts to retrieve a registered converter for a specific type.
        /// </summary>
        /// <param name="type">The type for which to retrieve the converter.</param>
        /// <param name="converter">When this method returns, contains the registered converter if found; otherwise, null.</param>
        /// <returns>True if a converter was found for the specified type; otherwise, false.</returns>
        public static bool TryGetConverter(Type type, out ITypeConverter? converter)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return _converters.TryGetValue(type, out converter);
        }

        /// <summary>
        /// Clears all registered converters. This is primarily intended for testing purposes.
        /// </summary>
        public static void Clear()
        {
            _converters.Clear();
        }
    }
}
