using System.Text.Json;

namespace Test.Utils;

public static class HttpHelper
{
    private static readonly HttpClient Client = new();

    /// <summary>
    /// Fetches data from a list of URLs asynchronously.
    /// </summary>
    /// <param name="urls">The list of URLs to fetch data from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of strings with the fetched data.</returns>
    private static async Task<List<string>> FetchDataFromUrlsAsync(IEnumerable<string> urls)
    {
        var results = await Task.WhenAll(urls.Select(async url =>
        {
            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }));

        return results.ToList();
    }

    /// <summary>
    /// Prepares data by fetching it from predefined URLs and deserializing the JSON responses.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of dictionaries with the deserialized data.</returns>
    public static async Task<List<Dictionary<string, object>>> PrepareDataAsync()
    {
        List<string> urls =
        [
            "https://5f2be0b4ffc88500167b85a0.mockapi.io/suppliers/acme",
            "https://5f2be0b4ffc88500167b85a0.mockapi.io/suppliers/patagonia",
            "https://5f2be0b4ffc88500167b85a0.mockapi.io/suppliers/paperflies"
        ];
        var results = await FetchDataFromUrlsAsync(urls);

        List<Dictionary<string, object>> hotels = [];
        foreach (var result in results)
            hotels.AddRange(JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result));
        return hotels;
    }
}