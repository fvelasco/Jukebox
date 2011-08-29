using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Jukebox.Models
{
    [XmlRoot("artist")]
    public class ArtistModel
    {
        public ArtistModel()
        {
            //Albums = new List<AlbumModel>();
        }

        //[XmlAttribute("id")]
        //public Guid Id { get; set; }

        //[XmlAttribute("indexName")]
        //public string IndexName { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        //[XmlElement("albums")]
        //public List<AlbumModel> Albums { get; set; }
    }
}
