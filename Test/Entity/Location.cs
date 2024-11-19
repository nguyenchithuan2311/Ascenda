namespace Test.Entity;

public class Location
{
    [Extension.Extension.JsonField("Latitude")]
    public float Lat { get; set; }
    [Extension.Extension.JsonField("Longitude")]
    public float Lng { get; set; }
    
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}
