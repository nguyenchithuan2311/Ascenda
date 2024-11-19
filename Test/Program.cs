using System.Collections.Immutable;
using System.Text.Json;
using Test.Entity;
using Test.Utils;

namespace Test;

class Program
{ 
    public static async Task Main(string[] args)
    {
        var hotels = await HttpHelper.PrepareDataAsync();
        if (hotels != null)
        {
            var results = new List<OriginalHotel>();
           Converter.Converter.ConvertObject(hotels, results);
           results.Sort((x, y) => string.Compare(x.Id, y.Id, StringComparison.Ordinal));
           var count = 0;
           for (var i = 1; i < results.Count; i++)
           {
               if (results[count].Id != results[i].Id)
               {
                   count = i;
               }
               else
               {
                   MergerObject.MergeObjects(results[count], results[i]);
                   results.Remove(results[i]);
                   i -= 1;
               }
           }
           
           Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        }
        
    }
}
