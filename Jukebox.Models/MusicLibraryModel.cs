using System.Collections.Generic;
using System.Xml.Serialization;

namespace Jukebox.Models
{
    [XmlRoot("musicLibrary")]
    public class MusicLibraryModel
    {
        public MusicLibraryModel()
        {
            Artists = new List<ArtistModel>();
        }

        [XmlElement("artist")]
        public List<ArtistModel> Artists { get; set; }
    }
}
