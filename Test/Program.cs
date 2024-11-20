using System.Text.Json;
using Test.Entity;
using Test.Utils;

namespace Test;

class Program
{
    public static async Task Main(string[] args)
    {
        // Extracts hotel IDs and destination IDs from the input arguments.
        Utils.Utils.ExtractInputAndOutput(args, out var hotelIds, out var destinationIds);

        // Prepares the hotel data asynchronously.
        var hotels = await HttpHelper.PrepareDataAsync();

        if (hotels.Count > 0)
        {
            // Initializes a list to store the results.
            var results = new List<OriginalHotel>();

            // Converts the hotel data to the OriginalHotel type.
            Converter.Converter.ConvertObject(hotels, results);

            // Filters the results by hotel IDs and destination IDs.
            results = SortAndFilter.FilterHotelsByHotelIdsAndDestinationIds(hotelIds, destinationIds, results);

            // Sorts the results by hotel IDs and destination IDs.
            SortAndFilter.SortHotelsByHotelIdsAndDestinationIds(hotelIds, destinationIds, results);

            var count = 0;
            for (var i = 1; i < results.Count; i++)
            {
                if (results[count].Id != results[i].Id)
                    count = i;
                else
                {
                    // Merges duplicate hotel entries.
                    MergerObject.MergeObjects(results[count], results[i]);
                    results.Remove(results[i]);
                    i -= 1;
                }
            }

            // Serializes the results to JSON and prints them to the console.
            Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}