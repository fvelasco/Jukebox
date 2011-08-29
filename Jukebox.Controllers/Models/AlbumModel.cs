using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Jukebox.Models
{
    public class AlbumModel
    {
        public AlbumModel()
        {
            Tracks = new List<TrackModel>();
        }

        [XmlAttribute("id")]
        public Guid Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("image")]
        public string Image { get; set; }

        [XmlAttribute("genre")]
        public string Genre { get; set; }

        [XmlElement(ElementName = "year", IsNullable = true)]
        public short? Year { get; set; }

        [XmlElement("track")]
        public List<TrackModel> Tracks { get; set; }

        [XmlIgnore]
        public List<TrackModel> TracksOrderedByNumber
        {
            get { return Tracks.OrderBy(track => track.Number).ToList(); }
        }
    }
}
