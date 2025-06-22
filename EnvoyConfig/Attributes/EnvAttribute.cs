namespace EnvoyConfig.Attributes
{
    using System;

    /// <summary>
    /// Specifies the environment variable to map to a class property
    /// when using <see cref="EnvConfig.Load{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EnvAttribute : Attribute
    {
        /// <summary>
        /// Gets the name (key) of the environment variable to read from.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the default value to use if the environment variable
        /// specified by <see cref="Key"/> is not found or is empty.
        /// The value must be convertible to the target property's type.
        /// </summary>
        public object? Default { get; set; }

        /// <summary>
        /// Gets or sets the default value to use if the environment variable
        /// specified by <see cref="Key"/> is not found or is empty.
        /// The value must be convertible to the target property's type.
        /// This is an alias for <see cref="Default"/> to support the more conventional naming pattern.
        /// </summary>
        public object? DefaultValue
        {
            get => Default;
            set => Default = value;
        }

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
        /// Example: MYAPP_ZONE_1_MQTT_CONTROL_SET_TOPIC, MYAPP_ZONE_2_MQTT_CONTROL_SET_TOPIC, etc.
        /// </summary>
        public string? NestedListPrefix { get; set; }

        /// <summary>
        /// Suffix after the index for NestedListPrefix. Example: for MYAPP_ZONE_1_MQTT_, prefix is ZONE_, suffix is _MQTT_.
        /// </summary>
        public string? NestedListSuffix { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvAttribute"/> class with no key.
        /// Allows use with ListPrefix, MapPrefix, etc.
        /// </summary>
        public EnvAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvAttribute"/> class.
        /// </summary>
        /// <param name="key">The name (key) of the environment variable.</param>
        /// <exception cref="ArgumentNullException">Thrown if key is null or empty.</exception>
        public EnvAttribute(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            Key = key;
        }
    }

    /// <summary>
    /// Specifies casing options for environment variable map keys.
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
}
