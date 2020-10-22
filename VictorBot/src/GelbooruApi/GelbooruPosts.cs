using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GelbooruApi
{
    [XmlRoot(ElementName = "posts")]
    public partial class GelbooruPosts
    {
        [XmlElement("post")]
        public GelbooruPost[] Posts { get; set; }

        [XmlAttribute("count")]
        public uint Count { get; set; }

        [XmlAttribute("offset")]
        public uint Offset { get; set; }
    }
}
