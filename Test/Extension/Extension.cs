using System.ComponentModel;

namespace Test.Extension;

public static class Extension
{
    public static object? ConvertToType(this string? input, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(input)) 
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        return TypeDescriptor.GetConverter(targetType).ConvertFromString(input);
    }
}