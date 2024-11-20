using System.ComponentModel;

namespace Test.Extension;

/// <summary>
/// Provides extension methods for type conversion and custom JSON field attributes.
/// </summary>
public static class Extension
{
    /// <summary>
    /// Converts a string to a specified target type.
    /// </summary>
    /// <param name="input">The input string to be converted.</param>
    /// <param name="targetType">The type to which the input string will be converted.</param>
    /// <returns>The converted object of the specified target type, or null if the input is null or whitespace.</returns>
    public static object? ConvertToType(this string? input, Type targetType)
    {
        if (input == null || string.IsNullOrWhiteSpace(input))
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        return TypeDescriptor.GetConverter(targetType).ConvertFromString(input);
    }

    /// <summary>
    /// Specifies a custom JSON field name for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class JsonFieldAttribute(string fieldName) : Attribute
    {
        /// <summary>
        /// Gets the custom JSON field name.
        /// </summary>
        public string FieldName { get; } = fieldName;
    }
}