namespace EnvoyConfig.Attributes
{
    using System;

    /// <summary>
    /// Specifies alternative string values that can be used to parse this enum member
    /// from environment variables. Supports multiple aliases for the same enum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnvValueAttribute : Attribute
    {
        /// <summary>
        /// Gets the array of string values that can be used to parse this enum member.
        /// </summary>
        public string[] Values { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvValueAttribute"/> class.
        /// </summary>
        /// <param name="values">The string values that can be used to parse this enum member.</param>
        /// <exception cref="ArgumentNullException">Thrown if values is null.</exception>
        /// <exception cref="ArgumentException">Thrown if values is empty.</exception>
        public EnvValueAttribute(params string[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                throw new ArgumentException("At least one value must be specified.", nameof(values));
            
            Values = values;
        }
    }
}
