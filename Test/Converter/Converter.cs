using System.Collections;
using System.Reflection;
using System.Text.Json;
using Test.Extension;
using Test.Utils;

namespace Test.Converter;

public abstract class Converter
{
    /// <summary>
    /// Handles the deserialization and assignment of JSON arrays to the target object's property.
    /// </summary>
    /// <param name="targetObject">The object whose property will be set.</param>
    /// <param name="matchingKey">The key used to find the matching property on the target object.</param>
    /// <param name="inputKey">The key from the input JSON.</param>
    /// <param name="json">The JSON string to be deserialized.</param>
    /// <param name="propertyType">The type of the property to be set on the target object.</param>
    private static void HandleJsonArray(object targetObject, string matchingKey, string inputKey, string json,
        Type propertyType)
    {
        // Deserialize the JSON string into a list of objects
        var genericList = JsonSerializer.Deserialize<List<object>>(json);
        if (genericList != null && genericList.All(item => item is JsonElement
            {
                ValueKind: JsonValueKind.Object
            }))
        {
            // Deserialize the JSON string into a list of objects
            var newList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            if (newList == null) return;
            // Find the property on the target object using the matching key or input key
            var children = targetObject.GetType().GetProperty(matchingKey) ??
                           targetObject.GetType().GetProperty($"{char.ToUpper(inputKey[0])}{inputKey[1..]}");

            if (children == null) return;

            // Find the property on the target object using the matching key or input key
            var isGenericList = children.PropertyType.IsGenericType &&
                                children.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
            // Find the property on the target object using the matching key or input key
            var listType = isGenericList
                ? children.PropertyType
                : typeof(List<>).MakeGenericType(children.PropertyType);
            if (Activator.CreateInstance(listType) is not IList listInstance) return;
            // Convert the list of dictionaries to the target list type
            ConvertObject(newList, listInstance);
            // Set the value of the property on the target object
            children.SetValue(targetObject, isGenericList ? listInstance : listInstance[0]);
        }
        else
        {
            try
            {
                // Deserialize the JSON string into a list of the element type
                var elementType = propertyType.GetGenericArguments()[0];
                var simpleList = JsonSerializer.Deserialize(json, typeof(List<>).MakeGenericType(elementType));
                // Set the value of the property on the target object
                targetObject.GetType().GetProperty(matchingKey)?.SetValue(targetObject, simpleList);
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// Handles the deserialization and assignment of a JSON object to the target object's property.
    /// </summary>
    /// <param name="targetObject">The object whose property will be set.</param>
    /// <param name="matchingKey">The key used to find the matching property on the target object.</param>
    /// <param name="inputKey">The key from the input JSON.</param>
    /// <param name="json">The JSON string to be deserialized.</param>
    private static void HandleJsonObject(object targetObject, string matchingKey, string inputKey, string json)
    {
        // Deserialize the JSON string into a dictionary
        var newDic = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        if (newDic == null) return;

        // Find the property on the target object using the matching key or input key
        var children = targetObject.GetType().GetProperty(matchingKey) ??
                       targetObject.GetType().GetProperty(inputKey);
        if (children == null) return;

        // Create an instance of the list type
        var listType = typeof(List<>).MakeGenericType(children.PropertyType);
        if (Activator.CreateInstance(listType) is not IList listInstance) return;

        // Convert the dictionary to the target list type
        ConvertObject([newDic], listInstance);

        // Set the value of the property on the target object
        children.SetValue(targetObject, listInstance[0]);
    }

    /// <summary>
    /// Handles the deserialization and assignment of a complex JSON type to the target object's property.
    /// </summary>
    /// <param name="targetObject">The object whose property will be set.</param>
    /// <param name="matchingKey">The key used to find the matching property on the target object.</param>
    /// <param name="inputKey">The key from the input JSON.</param>
    /// <param name="value">The value to be deserialized and assigned.</param>
    /// <param name="propertyType">The type of the property to be set on the target object.</param>
    private static void HandleComplexType(object targetObject, string matchingKey, string inputKey, object value,
        Type propertyType)
    {
        // Convert the value to a string
        var temp = value.ToString();

        // Check if the string represents a JSON array
        if (temp != null && temp.TrimStart().StartsWith("["))
        {
            // Handle the JSON array
            HandleJsonArray(targetObject, matchingKey, inputKey, temp, propertyType);
        }
        else
        {
            // Handle the JSON object
            if (temp != null) HandleJsonObject(targetObject, matchingKey, inputKey, temp);
        }
    }

    /// <summary>
    /// Handles the deserialization and assignment of a simple type to the target object's property.
    /// </summary>
    /// <param name="targetObject">The object whose property will be set.</param>
    /// <param name="matchingKey">The key used to find the matching property on the target object.</param>
    /// <param name="value">The value to be deserialized and assigned.</param>
    /// <param name="matchingProperty">The property information of the matching property on the target object.</param>
    private static void HandleSimpleType(object targetObject, string matchingKey, object value,
        PropertyInfo matchingProperty)
    {
        // Convert the value to the type of the matching property
        var convertedValue = value.ToString().ConvertToType(matchingProperty.PropertyType);
        if (convertedValue == null) return;

        // Find the key in the list of all target types
        var containsKey = Utils.Utils.AllTargetType.FirstOrDefault(s => s.Key.Contains(matchingKey));
        if (containsKey.Value == null) return;

        // Get the name of the key and the property information of the matching key
        var keyName = containsKey.Value.Name;
        var targetType = containsKey.Value.GetProperty(matchingKey);
        var objectTypeParent = Utils.Utils.AllTargetType.FirstOrDefault(s => s.Key.Contains(keyName)).Value
            ?.GetProperty(keyName);
        var children = targetObject.GetType().GetProperty(matchingKey);

        try
        {
            // Set the value of the property on the target object
            children.SetValue(targetObject, convertedValue);
        }
        catch (Exception)
        {
            // Handle exceptions by setting the value on the old object and updating the target object
            var oldObject = targetObject.GetType().GetProperty(keyName)?.GetValue(targetObject);
            targetType.SetValue(oldObject, convertedValue);
            var targetProperty = objectTypeParent.PropertyType == targetObject.GetType() ? matchingKey : keyName;
            targetObject.GetType().GetProperty(targetProperty).SetValue(targetObject, oldObject);
        }
    }

    /// <summary>
    /// Converts a list of dictionaries to a target list of objects by matching and setting properties.
    /// </summary>
    /// <param name="inputList">The list of dictionaries containing the input data.</param>
    /// <param name="objectTarget">The target list of objects to which the data will be assigned.</param>
    public static void ConvertObject(List<Dictionary<string, object>> inputList, IList objectTarget)
    {
        var i = -1;
        foreach (var input in inputList)
        {
            i += 1;
            // Ensure the target list has enough capacity to hold the new object
            Validation.EnsureObjectTargetCapacity(objectTarget, i);
            foreach (var property in input.Keys)
            {
                // Find the matching key in the target object
                var matchingKey = Utils.Utils.FindKeyContainingSubstring(property);
                if (matchingKey == null) continue;

                // Get the input key from the input dictionary
                var inputKey = Validation.GetInputKey(input, property, matchingKey);
                if (inputKey == null) continue;

                // Try to get the value associated with the input key
                Validation.TryGetInputValue(input, inputKey, matchingKey, out var value);
                if (value == null) continue;

                // Find the matching property in the target object
                var matchingProperty = Utils.Utils.FindMatchingProperty(matchingKey);
                var propertyType = matchingProperty?.PropertyType;
                if (propertyType is { IsClass: true } && propertyType != typeof(string))
                {
                    // Handle complex types (e.g., JSON objects or arrays)
                    HandleComplexType(objectTarget[i], matchingKey, inputKey, value, propertyType);
                }
                else
                {
                    // Handle simple types (e.g., strings, integers)
                    HandleSimpleType(objectTarget[i], matchingKey, value, matchingProperty);
                }
            }
        }
    }
}