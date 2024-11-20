using System.Reflection;
using Test.Entity;

namespace Test.Utils;

public abstract class Utils
{
    /// <summary>
    /// A list of all properties of the OriginalHotel type.
    /// </summary>
    private static readonly List<PropertyInfo> AllProperties = GetAllProperties(typeof(OriginalHotel));

    /// <summary>
    /// A list of dictionaries containing custom attributes for the OriginalHotel type.
    /// </summary>
    private static readonly List<Dictionary<string, string>> AllCustomAttributes =
        GetAllCustomAttributes(typeof(OriginalHotel));

    /// <summary>
    /// A dictionary mapping property names to their types for the OriginalHotel type.
    /// </summary>
    public static readonly Dictionary<string, Type> AllTargetType = GetAllPropertiesWithParent(typeof(OriginalHotel));

    /// <summary>
    /// Retrieves all properties of a given type, including nested properties.
    /// </summary>
    /// <param name="type">The type to retrieve properties from.</param>
    /// <returns>A list of PropertyInfo objects representing the properties of the given type.</returns>
    private static List<PropertyInfo> GetAllProperties(Type type)
    {
        List<PropertyInfo> properties = [];

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            properties.Add(prop);
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                properties.AddRange(GetAllProperties(prop.PropertyType));
        }

        return properties;
    }

    /// <summary>
    /// Retrieves all custom attributes of a given type, including nested properties.
    /// </summary>
    /// <param name="type">The type to retrieve custom attributes from.</param>
    /// <returns>A list of dictionaries containing custom attributes for the given type.</returns>
    private static List<Dictionary<string, string>> GetAllCustomAttributes(Type type)
    {
        var customAttributes = new List<Dictionary<string, string>>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attributes =
                prop.GetCustomAttributes(typeof(Extension.Extension.JsonFieldAttribute),
                    false);

            var attributeDict = new Dictionary<string, string>();

            foreach (var attribute in attributes)
            {
                if (attribute is Extension.Extension.JsonFieldAttribute jsonFieldAttribute)
                {
                    attributeDict[jsonFieldAttribute.FieldName] = prop.Name;
                }
            }

            if (attributeDict.Count > 0)
            {
                customAttributes.Add(attributeDict);
            }

            if (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string)) continue;
            var nestedProperties = GetAllCustomAttributes(prop.PropertyType);
            customAttributes.AddRange(nestedProperties);
        }

        return customAttributes;
    }

    /// <summary>
    /// Retrieves all properties of a given type along with their parent types, including nested properties.
    /// </summary>
    /// <param name="type">The type to retrieve properties from.</param>
    /// <returns>A dictionary mapping property names to their parent types.</returns>
    private static Dictionary<string, Type> GetAllPropertiesWithParent(Type type)
    {
        var propertyDict = new Dictionary<string, Type>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var key = $"{Guid.NewGuid()}_{prop.Name}";
            propertyDict[key] = type;

            if (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string)) continue;
            foreach (var nestedProperty in GetAllPropertiesWithParent(prop.PropertyType))
            {
                propertyDict[$"{Guid.NewGuid()}_{nestedProperty.Key}"] = prop.PropertyType;
            }
        }

        return propertyDict;
    }

    /// <summary>
    /// Finds a key in the properties or custom attributes that contains the specified substring.
    /// </summary>
    /// <param name="substring">The substring to search for.</param>
    /// <returns>The name of the property or custom attribute that contains the substring, or null if not found.</returns>
    public static string? FindKeyContainingSubstring(string substring)
    {
        substring = substring.ToLower().Replace("_", "");
        var result = AllProperties
            .Select(prop => new
            {
                Property = prop,
                MatchCount = GetMatchCount(substring.ToLower(), prop.Name.ToString().ToLower())
            }).Where(s => s.MatchCount > 0)
            .MaxBy(match => match.MatchCount)?.Property.Name ?? AllCustomAttributes
            .SelectMany(s => s)
            .Select(prop => new
            {
                Property = prop.Value,
                MatchCount = GetMatchCount(substring.ToLower(), prop.Key.ToLower())
            }).Where(s => s.MatchCount > 0)
            .MaxBy(match => match.MatchCount)?.Property;
        return result;
    }

    /// <summary>
    /// Gets the match count of a substring within a property name.
    /// </summary>
    /// <param name="substring">The substring to search for.</param>
    /// <param name="propertyName">The property name to search within.</param>
    /// <returns>The number of times the substring appears in the property name.</returns>
    private static int GetMatchCount(string substring, string propertyName)
    {
        var matchCount = 0;
        var index = 0;

        while ((index = propertyName.IndexOf(substring, index, StringComparison.CurrentCultureIgnoreCase)) != -1)
        {
            matchCount++;
            index += substring.Length;
        }

        index = 0;
        while ((index = substring.IndexOf(propertyName, index, StringComparison.CurrentCultureIgnoreCase)) != -1)
        {
            matchCount++;
            index += propertyName.Length;
        }

        return matchCount;
    }

    /// <summary>
    /// Finds a property that matches the specified key.
    /// </summary>
    /// <param name="matchingKey">The key to match against property names.</param>
    /// <returns>The PropertyInfo object that matches the key, or null if not found.</returns>
    public static PropertyInfo? FindMatchingProperty(string matchingKey)
    {
        return AllProperties.FirstOrDefault(s =>
            s.Name.Contains(matchingKey, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Extracts hotel IDs and destination IDs from the input arguments.
    /// </summary>
    /// <param name="args">The input arguments containing hotel IDs and destination IDs.</param>
    /// <param name="hotelIds">The extracted list of hotel IDs.</param>
    /// <param name="destinationIds">The extracted list of destination IDs.</param>
    public static void ExtractInputAndOutput(string[] args, out List<string> hotelIds, out List<string> destinationIds)
    {
        var isIntersectionFound = false;
        hotelIds = [];
        destinationIds = [];
        foreach (var arg in args)
        {
            if (isIntersectionFound == false)
            {
                hotelIds.AddRange(arg.Split(',').ToList());
                isIntersectionFound = true;
            }
            else
            {
                destinationIds.AddRange(arg.Split(',').ToList());
            }
        }
    }
}