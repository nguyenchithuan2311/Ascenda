using System.Reflection;

namespace Test.Utils;

public class MergerObject
{
    public static void MergeObjects<T>(T objA, T objB) where T : class, new()
    {
        if (objA == null || objB == null)
            throw new ArgumentNullException("Objects cannot be null");

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            var valueB = property.GetValue(objB);
            var valueA = property.GetValue(objA);

            if (IsDefaultValue(valueB) || valueB == null) continue;

            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                if (IsDefaultValue(objA.GetType().GetProperty(property.Name).GetValue(objA)))
                {
                    var newValue = Activator.CreateInstance(property.PropertyType);
                    newValue = property.GetValue(objB);
                    property.SetValue(objA, newValue);
                }
                else
                {
                    MergeObjects(valueA, valueB, property.PropertyType);
                }

                
            }
            else if (IsDefaultValue(objA) || objA == null)
            {
                property.SetValue(objA, valueB);
            }
        }
    }

    private static void MergeObjects(object objA, object objB, Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            try
            {
                var valueB = property.GetValue(objB);
                var valueA = property.GetValue(objA);
                if (valueB == null || IsDefaultValue(valueB)) continue;
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    if (IsDefaultValue(objA.GetType().GetProperty(property.Name).GetValue(objA)))
                    {
                        property.SetValue(objA, valueB);
                    }
                    else
                    {
                        MergeObjects(valueA, valueB, property.PropertyType);
                    }
                }
                else if (IsDefaultValue(objA) || objA == null)
                {
                    property.SetValue(objA, valueB);
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    private static bool IsDefaultValue(object? value)
    {
        if (value == null)
            return true;

        var type = value.GetType();

        if (type.IsValueType)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double) ||
                type == typeof(decimal))
            {
                return Convert.ToDecimal(value) == 0;
            }

            if (type == typeof(bool))
            {
                return (bool)value == false;
            }

            return value.Equals(Activator.CreateInstance(type));
        }

        if (type == typeof(string))
        {
            return string.IsNullOrEmpty((string)value);
        }

        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            var list = value as System.Collections.IEnumerable;
            return list == null || !list.GetEnumerator().MoveNext();
        }


        if (!type.IsClass) return false;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return (from property in properties where property.CanRead && property.CanWrite select property.GetValue(value))
            .All(IsDefaultValue);
    }
}