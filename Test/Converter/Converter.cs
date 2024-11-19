using System.Collections;
using System.Reflection;
using System.Text.Json;
using Test.Extension;
using Test.Utils;

namespace Test.Converter;

public class Converter
    { 
        private static void HandleJsonArray(object targetObject, string matchingKey, string inputKey, string json, Type propertyType)
        {
            var genericList = JsonSerializer.Deserialize<List<object>>(json);
            if (genericList != null && genericList.All(item => item is JsonElement
                {
                    ValueKind: JsonValueKind.Object
                }))
            {
                var newList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
                if (newList == null) return;

                var children = targetObject?.GetType().GetProperty(matchingKey) ??
                               targetObject?.GetType()
                                   .GetProperty($"{char.ToUpper(inputKey[0])}{inputKey[1..]}");

                if (children != null && children.PropertyType.IsGenericType &&
                    children.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listInstance = Activator.CreateInstance(children.PropertyType) as IList;
                    ConvertObject(newList, listInstance);
                    children.SetValue(targetObject, listInstance);
                }
                else
                {
                    var listInstance =
                        Activator.CreateInstance(typeof(List<>).MakeGenericType(children.PropertyType)) as IList;
                    ConvertObject(newList, listInstance);
                    children.SetValue(targetObject, listInstance[0]);
                }
            }
            else
            {
                try
                {
                    var simpleList = JsonSerializer.Deserialize(json,
                        typeof(List<>).MakeGenericType(propertyType.GetGenericArguments()[0]));
                    var children = targetObject?.GetType().GetProperty(matchingKey);
                    children?.SetValue(targetObject, simpleList);
                }
                catch
                {
                    // ignored
                }
            }
        }
        private static void HandleJsonObject(object targetObject, string matchingKey, string inputKey, string json)
        {
            var newDic = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (newDic == null) return;
            
            var children = targetObject?.GetType().GetProperty(matchingKey) ??
                           targetObject?.GetType().GetProperty(inputKey);
            
            var listType = typeof(List<>).MakeGenericType(children.PropertyType);
            var listInstance = Activator.CreateInstance(listType) as IList;
            ConvertObject([newDic], listInstance);
            children.SetValue(targetObject, listInstance[0]);
        }
        private static void HandleComplexType(object targetObject, string matchingKey, string inputKey, object value, Type propertyType)
        {
            var temp = value.ToString();
            if (temp.TrimStart().StartsWith("["))
            {
                HandleJsonArray(targetObject, matchingKey, inputKey, temp, propertyType);
            }
            else
            {
                HandleJsonObject(targetObject, matchingKey, inputKey, temp);
            }
        }
        
        private static void HandleSimpleType(object targetObject, string matchingKey, object value, PropertyInfo matchingProperty)
        {
            var convertedValue = value.ToString().ConvertToType(matchingProperty.PropertyType);
            if (convertedValue == null) return;
            
            var containsKey = Utils.Utils.AllTargetType.FirstOrDefault(s => s.Key.Contains(matchingKey));
            var keyName = containsKey.Value.Name;
            var targetType = containsKey.Value.GetProperty(matchingKey);
            var objectTypeParent = Utils.Utils.AllTargetType.FirstOrDefault(s => s.Key.Contains(keyName)).Value?.GetProperty(keyName);
            var children = targetObject.GetType().GetProperty(matchingKey);
            
            try
            {
                children.SetValue(targetObject, convertedValue);
            }
            catch (Exception)
            {
                var oldObject = targetObject.GetType().GetProperty(keyName).GetValue(targetObject);
                targetType.SetValue(oldObject, convertedValue);
                var targetProperty = objectTypeParent.PropertyType == targetObject.GetType() ? matchingKey : keyName;
                targetObject.GetType().GetProperty(targetProperty).SetValue(targetObject, oldObject);
            }
        }
        
        public static void ConvertObject(List<Dictionary<string, object>> inputList, IList objectTarget)
        {
            var i = -1;
            foreach (var input in inputList)
            {
                i += 1;
                Validation.EnsureObjectTargetCapacity(objectTarget, i);
                foreach (var property in input.Keys)
                {
                    var matchingKey = Utils.Utils.FindKeyContainingSubstring(property);
                    if (matchingKey == null) continue;
                    var inputKey = Validation.GetInputKey(input, property, matchingKey);
                    if (inputKey == null) continue;
                    
                    Validation.TryGetInputValue(input, inputKey, matchingKey, out var value);
                    if (value == null) continue;
                        
                    var matchingProperty = Utils.Utils.FindMatchingProperty(matchingKey);
                    var propertyType = matchingProperty?.PropertyType;
                    if (propertyType is { IsClass: true } && propertyType != typeof(string))
                    {
                        HandleComplexType(objectTarget[i], matchingKey, inputKey, value, propertyType);
                    }
                    else
                    {
                        HandleSimpleType(objectTarget[i], matchingKey, value, matchingProperty);
                    }
                }
            }
        }
    }