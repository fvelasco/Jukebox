using System;
using System.Xml.Serialization;

namespace Jukebox.Models
{
    public class TrackModel
    {
        [XmlAttribute("id")]
        public Guid Id { get; set; }

        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("indexName")]
        public string IndexName { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "number", IsNullable = true)]
        public short? Number { get; set; }

        [XmlAttribute("duration")]
        public double Duration { get; set; }

        [XmlIgnore]
        public string DurationString { get; set; }

        [XmlAttribute("image")]
        public string Image { get; set; }

        [XmlAttribute("genre")]
        public string Genre { get; set; }

        [XmlElement(ElementName = "year", IsNullable = true)]
        public short? Year { get; set; }

        [XmlIgnore]
        public string Artist { get; set; }

        [XmlIgnore]
        public string Album { get; set; }
    }
}
