using System.Collections;

namespace Test.Utils;

public abstract class Validation
{
    public static string? GetInputKey(Dictionary<string, object> input, string property, string matchingKey)
    {
        return input.Keys.FirstOrDefault(key =>
                   key.Contains(matchingKey, StringComparison.CurrentCultureIgnoreCase))
               ?? input.Keys.FirstOrDefault(key =>
                   key.Contains(property, StringComparison.CurrentCultureIgnoreCase));
    }
        
    public static void TryGetInputValue(Dictionary<string, object> input, string inputKey, string matchingKey, out object? value)
    {
        _ = input.TryGetValue(inputKey, out var valueObject) ||
            input.TryGetValue(inputKey.ToUpper(), out valueObject) ||
            input.TryGetValue(matchingKey, out valueObject);
        value = valueObject ?? null;
    }
        
    public static void EnsureObjectTargetCapacity(IList objectTarget, int index)
    {
        if (objectTarget.Count <= index)
        {
            objectTarget.Add(Activator.CreateInstance(objectTarget.GetType().GetGenericArguments()[0]));
        }
    }
}