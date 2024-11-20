using System.Text.Json;
using Test.Entity;
using Test.Utils;

namespace Test;

class Program
{
    public static async Task Main(string[] args)
    {
        Utils.Utils.ExtractInputAndOutput(args, out var hotelIds, out var destinationIds);
        var hotels = await HttpHelper.PrepareDataAsync();
        if (hotels.Count > 0)
        {
            var results = new List<OriginalHotel>();
            Converter.Converter.ConvertObject(hotels, results);
            results = SortAndFilter.FilterHotelsByHotelIdsAndDestinationIds(hotelIds, destinationIds, results);
            SortAndFilter.SortHotelsByHotelIdsAndDestinationIds(hotelIds, destinationIds, results);
            var count = 0;
            for (var i = 1; i < results.Count; i++)
            {
                if (results[count].Id != results[i].Id) count = i;
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