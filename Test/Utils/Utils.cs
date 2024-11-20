using System.Reflection;
using Test.Entity;

namespace Test.Utils;

public abstract class Utils
{
    private static readonly List<PropertyInfo> AllProperties = GetAllProperties(typeof(OriginalHotel));

    private static readonly List<Dictionary<string, string>> AllCustomAttributes =
        GetAllCustomAttributes(typeof(OriginalHotel));

    public static readonly Dictionary<string, Type> AllTargetType = GetAllPropertiesWithParent(typeof(OriginalHotel));

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


    public static PropertyInfo? FindMatchingProperty(string matchingKey)
    {
        return AllProperties.FirstOrDefault(s =>
            s.Name.Contains(matchingKey, StringComparison.CurrentCultureIgnoreCase));
    }

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