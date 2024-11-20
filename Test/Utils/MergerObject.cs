using System.Reflection;

namespace Test.Utils;

public abstract class MergerObject
{
    /// <summary>
    /// Merges the properties of two objects of the same type.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be merged.</typeparam>
    /// <param name="objA">The first object to merge.</param>
    /// <param name="objB">The second object to merge.</param>
    /// <exception cref="ArgumentNullException">Thrown when either objA or objB is null.</exception>
    public static void MergeObjects<T>(T objA, T objB) where T : class, new()
    {
        if (objA == null || objB == null)
            throw new ArgumentNullException(nameof(objA), "Objects cannot be null");

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            var valueB = property.GetValue(objB);
            var valueA = property.GetValue(objA);

            if (IsDefaultValue(valueB)) continue;

            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var valueAProperty = property.GetValue(objA);
                if (IsDefaultValue(valueAProperty))
                {
                    property.SetValue(objA, valueB);
                }
                else
                {
                    MergeObjects(valueA, valueB, property.PropertyType);
                }
            }
            else
            {
                property.SetValue(objA, valueB);
            }
        }
    }

    /// <summary>
    /// Merges the properties of two objects of the same type.
    /// </summary>
    /// <param name="objA">The first object to merge.</param>
    /// <param name="objB">The second object to merge.</param>
    /// <param name="type">The type of the objects to be merged.</param>
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
                if (valueB == null || IsDefaultValue(valueB)) continue;

                var valueA = property.GetValue(objA);
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    if (IsDefaultValue(valueA))
                    {
                        property.SetValue(objA, valueB);
                    }
                    else
                    {
                        MergeObjects(valueA, valueB, property.PropertyType);
                    }
                }
                else
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

    /// <summary>
    /// Determines whether the specified value is the default value for its type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is the default value for its type; otherwise, false.</returns>
    private static bool IsDefaultValue(object? value)
    {
        if (value == null)
            return true;

        var type = value.GetType();

        if (type.IsValueType)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double) ||
                type == typeof(decimal))
                return Convert.ToDecimal(value) == 0;

            if (type == typeof(bool))
                return (bool)value == false;

            return value.Equals(Activator.CreateInstance(type));
        }

        if (type == typeof(string))
            return string.IsNullOrEmpty((string)value);

        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            var list = value as System.Collections.IEnumerable;
            return list == null || !list.GetEnumerator().MoveNext();
        }

        if (!type.IsClass)
            return false;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return properties.Where(property => property is { CanRead: true, CanWrite: true })
            .Select(property => property.GetValue(value))
            .All(IsDefaultValue);
    }
}