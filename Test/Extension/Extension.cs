using System.ComponentModel;

namespace Test.Extension;

public static class Extension
{
    public static object? ConvertToType(this string? input, Type targetType)
    {
        if (input == null || string.IsNullOrWhiteSpace(input)) 
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        return TypeDescriptor.GetConverter(targetType).ConvertFromString(input);
    }
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class JsonFieldAttribute(string fieldName) : Attribute
    { 
        public string FieldName { get; } = fieldName;
    }
}