using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Jukebox.Models
{
    public class AlbumModel
    {
        //public AlbumModel()
        //{
        //    Tracks = new List<TrackModel>();
        //}

        [XmlAttribute("id")]
        public Guid Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("track")]
        public List<TrackModel> Tracks { get; set; }
    }
}
