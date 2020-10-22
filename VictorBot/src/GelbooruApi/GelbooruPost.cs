using System.Xml.Serialization;

namespace GelbooruApi
{
    public partial class GelbooruPost
    {
        [XmlAttribute()]
        public string height { get; set; }

        [XmlAttribute()]
        public string score { get; set; }

        [XmlAttribute()]
        public string file_url { get; set; }

        [XmlAttribute()]
        public string parent_id { get; set; }

        [XmlAttribute()]
        public string sample_url { get; set; }

        [XmlAttribute()]
        public string sample_width { get; set; }

        [XmlAttribute()]
        public string sample_height { get; set; }

        [XmlAttribute()]
        public string preview_url { get; set; }

        [XmlAttribute()]
        public string rating { get; set; }

        [XmlAttribute()]
        public string tags { get; set; }

        [XmlAttribute()]
        public string id { get; set; }

        [XmlAttribute()]
        public string width { get; set; }

        [XmlAttribute()]
        public string change { get; set; }

        [XmlAttribute()]
        public string md5 { get; set; }

        [XmlAttribute()]
        public string creator_id { get; set; }

        [XmlAttribute()]
        public string has_children { get; set; }

        [XmlAttribute()]
        public string created_at { get; set; }

        [XmlAttribute()]
        public string status { get; set; }

        [XmlAttribute()]
        public string source { get; set; }

        [XmlAttribute()]
        public string has_notes { get; set; }

        [XmlAttribute()]
        public string has_comments { get; set; }

        [XmlAttribute()]
        public string preview_width { get; set; }

        [XmlAttribute()]
        public string preview_height { get; set; }
    }
}
