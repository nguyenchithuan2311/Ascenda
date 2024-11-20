using Test.Entity;

namespace Test.Utils;

public abstract class SortAndFilter
{
    public static List<OriginalHotel> FilterHotelsByHotelIdsAndDestinationIds(List<string> hotelIds, List<string> destinationIds, List<OriginalHotel> results)
    {
        if (hotelIds.Count != 0 && hotelIds[0] != "none" && destinationIds.Count != 0 && destinationIds[0] != "none")
        {
            return results.Where(r =>
                hotelIds.Contains(r.Id) && destinationIds.Contains(r.DestinationId.ToString())).ToList();
        }

        return results;
    }

    public static void SortHotelsByHotelIdsAndDestinationIds(List<string> hotelIds, List<string> destinationIds, List<OriginalHotel> results)
    {
        var isHotelNone = hotelIds.Count != 0 && hotelIds[0] == "none";
        var isDestinationNone = destinationIds.Count != 0 && destinationIds[0] == "none";
        results.Sort((x, y) =>
        {
            var hotelOrderComparison = (hotelIds.Count != 0 && hotelIds[0] != "none")
                ? hotelIds.IndexOf(x.Id).CompareTo(hotelIds.IndexOf(y.Id))
                : 0;

            if (hotelOrderComparison != 0)
                return hotelOrderComparison;
            
            var destinationOrderComparison = destinationIds.Count != 0 && destinationIds[0] != "none"
                ? destinationIds.IndexOf(x.DestinationId.ToString()).CompareTo(destinationIds.IndexOf(y.DestinationId.ToString()))
                : 0;
            return destinationOrderComparison != 0 ? destinationOrderComparison :
                string.Compare(x.Id, y.Id, StringComparison.Ordinal);
        });
        if (isHotelNone || isDestinationNone) results.Reverse();
    }
}