namespace Test.Entity;


public class Images
{
    public List<TargetImageItem> Rooms { get; private set; } = [];
    public List<TargetImageItem> Site { get; private set; } = [];
    public List<TargetImageItem> Amenities { get; private set; } = [];
}

public class TargetImageItem
{
    public string Link { get; private set; }
    public string Description { get; private set; }
}