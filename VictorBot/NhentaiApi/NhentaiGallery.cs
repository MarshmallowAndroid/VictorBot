namespace VictorBot.NhentaiApi
{
    public class NhentaiGallery
    {
        public uint Id { get; set; }
        public string Media_Id { get; set; }
        public Title Title { get; set; }
        public Images Images { get; set; }
        public string Scanlator { get; set; }
        public long Upload_Date { get; set; }
        public Tag[] Tags { get; set; }
        public uint Num_Pages { get; set; }
        public uint Num_Favorites { get; set; }
    }

    public class Title
    {
        public string English { get; set; }
        public string Japanese { get; set; }
        public string Pretty { get; set; }
    }

    public class Images
    {
        public GalleryImage[] Pages { get; set; }
        public GalleryImage Cover { get; set; }
        public GalleryImage Thumbnail { get; set; }
    }

    public class GalleryImage
    {
        public string T { get; set; }
        public uint W { get; set; }
        public uint H { get; set; }

        public string ImageFormat { get { return GetImageFormat(); } }

        private string GetImageFormat()
        {
            switch (T)
            {
                case "j": return "jpg";
                case "p": return "png";
                case "g": return "gif";
                default: return "";
            }
        }
    }

    public class Tag
    {
        public uint Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public uint Count { get; set; }

        public TagType TagType
        {
            get
            {
                switch (Type)
                {
                    case "language": return TagType.Language;
                    case "parody": return TagType.Parody;
                    case "character": return TagType.Character;
                    case "tag": return TagType.Tag;
                    case "artist": return TagType.Artist;
                    case "group": return TagType.Group;
                    default: return TagType.Unknown;
                }
            }
        }
    }

    public enum TagType
    {
        Language,
        Parody,
        Character,
        Tag,
        Artist,
        Group,
        Unknown
    }
}
