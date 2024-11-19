using System.Text.Json;

namespace Test.Utils;

public static class HttpHelper
{
    private static readonly HttpClient Client = new();
    static async Task<List<string>> FetchDataFromUrlsAsync(IEnumerable<string> urls)
    {
        
        var tasks = urls.Select(async url =>
        {
            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        });
        
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
    
    public static async Task<List<Dictionary<string, object>>> PrepareDataAsync()
    {
        List<string> urls=[
            "https://5f2be0b4ffc88500167b85a0.mockapi.io/suppliers/acme",
            "https://5f2be0b4ffc88500167b85a0.mockapi.io/suppliers/patagonia",
            "https://5f2be0b4ffc88500167b85a0.mockapi.io/suppliers/paperflies"];
        var results = await FetchDataFromUrlsAsync(urls);
        
        List<Dictionary<string, object>> hotels = [];
        foreach (var result in results)
            hotels.AddRange(JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result));
        return hotels;
    }
}