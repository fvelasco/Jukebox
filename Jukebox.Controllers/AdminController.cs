using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Serialization;
using HundredMilesSoftware.UltraID3Lib;
using Jukebox.Models;

namespace Jukebox.Controllers
{
    public class AdminController : Controller
    {
        private MusicLibraryModel musicLibrary;
        private MusicLibraryModel MusicLibrary
        {
            get
            {
                return musicLibrary ?? (musicLibrary = new MusicLibraryModel());
            }
        }

        private string Path 
        {
            get { return HttpContext.Server.MapPath("~/ClientBin"); }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateIndex()
        {
            try
            {
                ClearImages();

                GetFiles(@"C:\\Users\\NathanBurn\\Music\\iTunes\\iTunes Media\\Music");

                var xmlSerializer = new XmlSerializer(typeof (MusicLibraryModel));
                using (TextWriter textWriter = new StreamWriter(string.Format(@"{0}\\MusicLibrary.xml", Path)))
                {
                    xmlSerializer.Serialize(textWriter, MusicLibrary);
                }

                return Json(new
                {
                    sMessage = "Created Index!"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.StackTrace));
            }
        }

        private void GetFiles(string path)
        {
            var rootDirectory = Directory.CreateDirectory(path);
            if (!rootDirectory.Exists) return;

            foreach (var directory in rootDirectory.GetDirectories())
            {
                GetFiles(directory.FullName);
            }

            foreach (var file in rootDirectory.GetFiles().Where(file => string.Equals(file.Extension, ".mp3", StringComparison.OrdinalIgnoreCase)))
            {
                AddTrack(file);
            }
        }

        private void ClearImages()
        {
            var rootDirectory = Directory.CreateDirectory(string.Format(@"{0}\\Images", Path));
            if (!rootDirectory.Exists) return;

            foreach(var file in rootDirectory.GetFiles()) 
                file.Delete();
        }

        private void AddTrack(FileInfo file)
        {
            try
            {
                var ultraId3 = new UltraID3();
                ultraId3.Read(file.FullName);

                var track = new TrackModel()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = ultraId3.Title,
                                    Genre = ultraId3.Genre,
                                    Duration = ultraId3.Duration.TotalSeconds,
                                    Number = ultraId3.TrackNum,
                                    Path = file.FullName,
                                    Year = ultraId3.Year
                                };

                var album = GetAlbum(ultraId3.Artist, ultraId3.Album);

                var id3PictureFrames = ultraId3.ID3v2Tag.Frames.GetFrames(MultipleInstanceID3v2FrameTypes.ID3v23Picture);
                if (id3PictureFrames != null && id3PictureFrames.Count > 0)
                {
                    track.Image = string.Format(@"{0}\\Images\\{1}.png", Path, album.Id);

                    if (!System.IO.File.Exists(track.Image))
                        (((ID3v23PictureFrame) id3PictureFrames[0]).Picture).Save(track.Image,
                                                                              System.Drawing.Imaging.ImageFormat.Png);
                }

                album.Year = ultraId3.Year;
                album.Genre = ultraId3.Genre;
                album.Image = track.Image;
                album.Tracks.Add(track);
            }
            catch (Exception ex)
            {
                // Log : File failed to be added
                //throw new Exception(string.Format("File failed to be added {0}", file.FullName), ex);
            }
            
        }

        private AlbumModel GetAlbum(string artistName, string albumName)
        {
            var artist = GetArtist(artistName);

            var album = artist.Albums.Where(
                a => string.Equals(a.Name, albumName, StringComparison.OrdinalIgnoreCase)).DefaultIfEmpty(null).FirstOrDefault();

            if (album == null)
            {
                album = new AlbumModel()
                {
                    Id = Guid.NewGuid(),
                    Name = albumName
                };
                artist.Albums.Add(album);
            }

            return album;
        }

        private ArtistModel GetArtist(string artistName)
        {
            var artist = MusicLibrary.Artists.Where(
                a => string.Equals(a.Name, artistName, StringComparison.OrdinalIgnoreCase)).DefaultIfEmpty(null).FirstOrDefault();

            if (artist == null)
            {
                artist = new ArtistModel()
                {
                    Id = Guid.NewGuid(),
                    IndexName = GetArtistIndexName(artistName),
                    Name = artistName
                };
                MusicLibrary.Artists.Add(artist);
            }

            return artist;
        }

        private string GetArtistIndexName(string artistName)
        {
            return Regex.Replace(artistName, @"^The ", "");
        }
    }
}
