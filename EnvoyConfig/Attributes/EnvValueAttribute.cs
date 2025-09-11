namespace EnvoyConfig.Attributes
{
    using System;

    /// <summary>
    /// Specifies alternative string values that can be used to parse this enum member
    /// from environment variables. Supports multiple aliases for the same enum value.
    /// </summary>
    [Obsolete("Use [EnvoyConfigValue] instead. [EnvValue] will be removed in a future version.", false)]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnvValueAttribute : EnvoyConfigValueAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvValueAttribute"/> class.
        /// </summary>
        /// <param name="values">The string values that can be used to parse this enum member.</param>
        /// <exception cref="ArgumentNullException">Thrown if values is null.</exception>
        /// <exception cref="ArgumentException">Thrown if values is empty.</exception>
        public EnvValueAttribute(params string[] values) : base(values) { }
    }
}
