using System.Reflection;
using Test.Entity;

namespace Test.Utils;

public class Utils
{
    public static readonly List<PropertyInfo> AllProperties = GetAllProperties(typeof(OriginalHotel));
    public static readonly Dictionary<string, Type> AllTargetType = GetAllPropertiesWithParent(typeof(OriginalHotel));

    private static List<PropertyInfo> GetAllProperties(Type type)
    {
        List<PropertyInfo> properties = [];

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            properties.Add(prop);

            if (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string)) continue;
            var nestedProperties = GetAllProperties(prop.PropertyType);
            properties.AddRange(nestedProperties);
        }

        return properties;
    }

    private static Dictionary<string, Type> GetAllPropertiesWithParent(Type type)
    {
        var propertyDict = new Dictionary<string, Type>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var key = $"{Guid.NewGuid()}_{prop.Name}";
            propertyDict[key] = type;

            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            {
                foreach (var nestedProperty in GetAllPropertiesWithParent(prop.PropertyType))
                {
                    propertyDict[$"{Guid.NewGuid()}_{nestedProperty.Key}"] = prop.PropertyType;
                }
            }
        }

        return propertyDict;
    }
    
    public static string? FindKeyContainingSubstring(string substring)
    {
        substring = substring.ToLower().Replace("_", "");
        var result = Utils.AllProperties
            .Select(prop => new
            {
                Property = prop,
                MatchCount = GetMatchCount(substring.ToLower(), prop.Name.ToString().ToLower()) // Đếm sự trùng khớp
            }).Where(s=>s.MatchCount>0)
            .MaxBy(match => match.MatchCount)?.Property.Name;
        return result;
    }

    private static int GetMatchCount(string substring, string propertyName)
    {
        int matchCount = 0;
        int index = 0;


        while ((index = propertyName.IndexOf(substring, index, StringComparison.CurrentCultureIgnoreCase)) != -1)
        {
            matchCount++;
            index += substring.Length; // Di chuyển chỉ mục qua vị trí kết thúc của substring tìm thấy
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

}