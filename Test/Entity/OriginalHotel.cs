namespace Test.Entity;

public class OriginalHotel
{
    public string Id { get; private set; }

    public int DestinationId { get; private set; }

    public string Name { get; private set; }

    public Location Location { get; private set; } = new();

    public string Description { get; private set; }

    public Amenities Amenities { get; private set; }=new();

    public Images Images { get; private set; }=new();

    public List<string> BookingConditions { get; private set; } = [];
}