using System.Xml.Serialization;

namespace GelbooruApi
{
    /// <summary>
    /// Class for Gelbooru API XML serialization
    /// </summary>
    [XmlRoot(ElementName = "posts")]
    public class GelbooruPosts
    {
        /// <summary>
        /// Array of posts in the request
        /// </summary>
        [XmlElement("post")]
        public GelbooruPost[] Posts { get; set; }

        /// <summary>
        /// Number of posts returned in the request
        /// </summary>
        [XmlAttribute("count")]
        public uint Count { get; set; }

        /// <summary>
        /// Offset of post list from count; useful for pagination
        /// </summary>
        [XmlAttribute("offset")]
        public uint Offset { get; set; }
    }
}
