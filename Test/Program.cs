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
        if (hotels != null)
        {
            var results = new List<OriginalHotel>();
           Converter.Converter.ConvertObject(hotels, results);
           results = SortAndFilter.FilterHotelsByHotelIdsAndDestinationIds(hotelIds, destinationIds, results);
           SortAndFilter.SortHotelsByHotelIdsAndDestinationIds(hotelIds, destinationIds, results);
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


// public class Hotel
// {
//     [JsonField("name_hotel")]
//     [JsonField("name")]
//     [JsonField("NameHotel")]
//     public string Name { get; set; }
//
//     public static Hotel FromJson(string json)
//     {
//         JObject jsonObject = JObject.Parse(json);
//         var hotel = new Hotel();
//
//         // Dùng reflection để lấy các thuộc tính có annotation JsonField
//         var property = typeof(Hotel).GetProperty("Name");
//         var attributes = property.GetCustomAttributes(typeof(JsonFieldAttribute), false) as JsonFieldAttribute[];
//
//         // Lặp qua các tên trường trong các attribute và lấy giá trị đầu tiên
//         foreach (var attribute in attributes)
//         {
//             var fieldValue = jsonObject[attribute.FieldName]?.ToString();
//             if (!string.IsNullOrEmpty(fieldValue))
//             {
//                 hotel.Name = fieldValue;
//                 break; // Dừng lại khi tìm thấy giá trị đầu tiên
//             }
//         }
//
//         return hotel;
//     }
// }
//
// public class Program
// {
//     public static void Main()
//     {
//         string json = @"{
//             'name': 'John Doe'
//         }";
//
//         Hotel hotel = Hotel.FromJson(json);
//         Console.WriteLine($"Name: {hotel.Name}");
//     }
// }