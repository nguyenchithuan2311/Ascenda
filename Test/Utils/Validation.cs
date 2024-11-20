using System.Collections;

namespace Test.Utils;

public abstract class Validation
{
    /// <summary>
    /// Retrieves the key from the input dictionary that matches the specified property or matching key.
    /// </summary>
    /// <param name="input">The input dictionary containing keys and values.</param>
    /// <param name="property">The property name to match against the keys.</param>
    /// <param name="matchingKey">The key to match against the keys.</param>
    /// <returns>The key from the input dictionary that matches the specified property or matching key, or null if no match is found.</returns>
    public static string? GetInputKey(Dictionary<string, object> input, string property, string matchingKey)
    {
        return input.Keys.FirstOrDefault(key =>
                   key.Contains(matchingKey, StringComparison.CurrentCultureIgnoreCase))
               ?? input.Keys.FirstOrDefault(key =>
                   key.Contains(property, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Tries to get the value from the input dictionary that matches the specified input key or matching key.
    /// </summary>
    /// <param name="input">The input dictionary containing keys and values.</param>
    /// <param name="inputKey">The key to match against the keys.</param>
    /// <param name="matchingKey">The key to match against the keys.</param>
    /// <param name="value">The value associated with the matched key, or null if no match is found.</param>
    public static void TryGetInputValue(Dictionary<string, object> input, string inputKey, string matchingKey,
        out object? value)
    {
        _ = input.TryGetValue(inputKey, out var valueObject) ||
            input.TryGetValue(inputKey.ToUpper(), out valueObject) ||
            input.TryGetValue(matchingKey, out valueObject);
        value = valueObject ?? null;
    }

    /// <summary>
    /// Ensures that the target list has the capacity to hold an object at the specified index.
    /// </summary>
    /// <param name="objectTarget">The target list to ensure capacity for.</param>
    /// <param name="index">The index to ensure capacity for.</param>
    public static void EnsureObjectTargetCapacity(IList objectTarget, int index)
    {
        if (objectTarget.Count <= index)
        {
            objectTarget.Add(Activator.CreateInstance(objectTarget.GetType().GetGenericArguments()[0]));
        }
    }
}