namespace EnvoyConfig.Attributes
{
    using System;

    /// <summary>
    /// Specifies the environment variable to map to a class property
    /// when using <see cref="EnvConfig.Load{T}"/>.
    /// </summary>
    [Obsolete("Use [EnvoyConfig] instead. [Env] will be removed in a future version.", false)]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EnvAttribute : EnvoyConfigAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvAttribute"/> class with no key.
        /// Allows use with ListPrefix, MapPrefix, etc.
        /// </summary>
        public EnvAttribute() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvAttribute"/> class.
        /// </summary>
        /// <param name="key">The name (key) of the environment variable.</param>
        /// <exception cref="ArgumentNullException">Thrown if key is null or empty.</exception>
        public EnvAttribute(string key) : base(key) { }
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
