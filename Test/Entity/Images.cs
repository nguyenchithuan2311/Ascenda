namespace Test.Entity;


public class Images
{
    public List<TargetImageItem> Rooms { get; private set; } = [];
    public List<TargetImageItem> Site { get; private set; } = [];
    public List<TargetImageItem> Amenities { get; private set; } = [];
}

public class TargetImageItem
{
    [Extension.Extension.JsonField("url")]
    public string Link { get; private set; }
    [Extension.Extension.JsonField("caption")]
    public string Description { get; private set; }
}