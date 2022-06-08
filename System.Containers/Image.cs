namespace System.Containers;

public class Image
{
    public int schemaVersion { get; set; }
    public Descriptor Config { get; set; }
    public Descriptor[] Layers { get; set; }
}

    public class Config
    {
        public string? mediaType { get; set; }
        public int size { get; set; }
        public string digest { get; set; }
    }
