using System;

namespace EnvoyConfig.Attributes
{
    /// <summary>
    /// Marks a property for environment variable initialization.
    /// </summary>
    public enum MapKeyCasingMode
    {
        /// <summary>Keys are converted to lower-case.</summary>
        Lower,

        /// <summary>Keys are converted to upper-case.</summary>
        Upper,

        /// <summary>Keys are left as-is.</summary>
        AsIs,
    }

    /// <summary>
    /// Attribute to control how environment variables are mapped to config properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EnvAttribute : Attribute
    {
        /// <summary>
        /// The environment variable key to map to this property.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The default value to use if the environment variable is not set.
        /// </summary>
        public string? Default { get; set; }

        /// <summary>
        /// If true, parses the value as a list (comma-separated by default).
        /// </summary>
        public bool IsList { get; set; }

        /// <summary>
        /// The separator character for list parsing (default: ',').
        /// </summary>
        public char ListSeparator { get; set; } = ',';

        /// <summary>
        /// Prefix for numbered list mode (e.g., PREFIX_1, PREFIX_2, ...).
        /// </summary>
        public string? ListPrefix { get; set; }

        /// <summary>
        /// Prefix for dictionary/map mode (e.g., PREFIX_key=value).
        /// </summary>
        public string? MapPrefix { get; set; }

        /// <summary>
        /// Controls how keys are cased for MapPrefix properties: Lower, Upper, or AsIs. Default is Lower.
        /// </summary>
        public MapKeyCasingMode MapKeyCasing { get; set; } = MapKeyCasingMode.Lower;

        /// <summary>
        /// Prefix for nested object mode (recursively prepends to child keys).
        /// </summary>
        public string? NestedPrefix { get; set; }

        /// <summary>
        /// When set, enables automatic discovery and population of a list of nested objects. All environment variables matching [NestedListPrefix][index][NestedListSuffix][key] will be grouped and used to create objects.
        /// Example: SNAPDOG_ZONE_1_MQTT_CONTROL_SET_TOPIC, SNAPDOG_ZONE_2_MQTT_CONTROL_SET_TOPIC, etc.
        /// </summary>
        public string? NestedListPrefix { get; set; }

        /// <summary>
        /// Suffix after the index for NestedListPrefix. Example: for SNAPDOG_ZONE_1_MQTT_, prefix is ZONE_, suffix is _MQTT_.
        /// </summary>
        public string? NestedListSuffix { get; set; }
    }
}
