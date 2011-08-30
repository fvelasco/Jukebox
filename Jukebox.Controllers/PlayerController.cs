using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml.Serialization;
using Jukebox.Models;
using WMPLib;

namespace Jukebox.Controllers
{
    public class PlayerController : Controller
    {
        private string Path
        {
            get { return HttpContext.Server.MapPath("~/ClientBin"); }
        }

        private MusicLibraryModel musicLibrary;
        private MusicLibraryModel MusicLibrary
        {
            get
            {
                if (musicLibrary == null)
                {
                    //musicLibrary = (MusicLibraryModel)HttpContext.Cache.Get("musicLibrary");
                    musicLibrary = (MusicLibraryModel)HttpContext.Application["musicLibrary"];      
                    if (musicLibrary == null)
                    {
                        musicLibrary = GetMusicLibrary(string.Format(@"{0}\\MusicLibrary.xml", Path));
                        //HttpContext.Cache.Insert("musicLibrary", musicLibrary, null, Cache.NoAbsoluteExpiration, new TimeSpan(1, 0, 0, 0), CacheItemPriority.High, null);
                        HttpContext.Application.Add("musicLibrary", musicLibrary);
                    }
                }
                return musicLibrary;
            }
        }

        private WMPLib.WindowsMediaPlayer player;
        public WMPLib.WindowsMediaPlayer Player
        {
            get
            {
                if(player == null)
                {
                    //player = (WMPLib.WindowsMediaPlayer)HttpContext.Cache.Get("player");
                    player = (WMPLib.WindowsMediaPlayer)HttpContext.Application["player"];
                    if(player == null)
                    {
                        player = new WMPLib.WindowsMediaPlayer();
                        //HttpContext.Cache.Insert("player", player, null, Cache.NoAbsoluteExpiration, new TimeSpan(1, 0, 0, 0), CacheItemPriority.High, null);  
                        HttpContext.Application.Add("player", player);
                    }
                }
                return player;
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        // AJAX: /Player/Stop
        public ActionResult Stop()
        {
            try
            {   
                Player.controls.stop();

                return Json(new
                {
                    sMessage = "Stopped!"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.InnerException));
            }
        }

        // AJAX: /Player/PlayOrPause
        public ActionResult PlayOrPause()
        {
            try
            {
                var message = "Playing!";
                if (Player.currentPlaylist != null && Player.currentPlaylist.count > 0)
                {
                    if (Player.playState.Equals(WMPLib.WMPPlayState.wmppsPlaying))
                    {
                        Player.controls.pause();
                        message = "Paused!";
                    }
                    else if (Player.playState.Equals(WMPLib.WMPPlayState.wmppsPaused))
                    {
                        Player.controls.play();
                    }
                }

                return Json(new
                {
                    sMessage = message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.InnerException));
            }
        }

        // AJAX: /Player/GetPlaylist
        public ActionResult GetPlaylist()
        {
            try
            {
                var tracks = new List<TrackModel>();

                for (var i = 0; i < Player.currentPlaylist.count; i++)
                {
                    var item = Player.currentPlaylist.get_Item(i);

                    short trackNumber = 1;
                    short.TryParse(item.getItemInfo("WM/TrackNumber"), out trackNumber);

                    short year = 2000;
                    short.TryParse(item.getItemInfo("WM/Year"), out year);

                    var track = new TrackModel()
                    {
                        Id = Guid.Empty,
                        Name = item.name,
                        Path = item.sourceURL,
                        Artist = item.getItemInfo("Author"),
                        Album = item.getItemInfo("WM/AlbumTitle"),
                        Number = trackNumber,
                        DurationString = item.durationString,
                        Year = year
                    };
                    tracks.Add(track);
                }

                return Json(new
                {
                    aaTracks = GetTrackJson(tracks)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.InnerException));
            }
        }

        private static IEnumerable<string[]> GetTrackJson(
            IEnumerable<TrackModel> tracks)
        {
            return tracks.Select(t => new[]
            {
                HttpUtility.HtmlEncode(t.Name),
                HttpUtility.UrlEncode(t.Path),
                HttpUtility.HtmlEncode(t.Artist),
                HttpUtility.HtmlEncode(t.Album),
                t.Number.ToString(),
                string.IsNullOrEmpty(t.DurationString)
                    ? string.Format("{0:D2}:{1:D2}", TimeSpan.FromSeconds(t.Duration).Minutes, TimeSpan.FromSeconds(t.Duration).Seconds) 
                    : t.DurationString,
                t.Year.ToString(),
                t.Id.ToString()
            }).ToList();
        }

        // AJAX: /Player/GetArtists
        public ActionResult GetArtists(string firstCharacter)
        {
            try
            {
                var artists = MusicLibrary.Artists.Where(a => a.IndexName.Length > 0 && 
                    a.IndexName[0].ToString().ToLower() == firstCharacter.ToLower());

                return Json(new
                {
                    aaArtists = GetArtistsJson(artists)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.InnerException));
            }
        }

        private static IEnumerable<string[]> GetArtistsJson(
            IEnumerable<ArtistModel> artists)
        {
            return artists.Select(a => new[]
            {
                HttpUtility.HtmlEncode(a.Name),
                a.Id.ToString()
            }).ToList();
        }

        // AJAX: /Player/GetAlbums
        public ActionResult GetAlbums(string artistId)
        {
            try
            {
                var albums = MusicLibrary.Artists.Where(a => a.Id == new Guid(artistId)).First().Albums.OrderByDescending(a => a.Year);

                return Json(new
                {
                    aaAlbums = GetAlbumsJson(albums)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.InnerException));
            }
        }

        private static IEnumerable<object> GetAlbumsJson(
            IEnumerable<AlbumModel> albums)
        {
            return albums.Select(a => new
            {
                sName = HttpUtility.HtmlEncode(a.Name),
                gId = a.Id.ToString(),
                dYear = a.Year.ToString(),
                sGenre = a.Genre,
                sImage = string.IsNullOrEmpty(a.Image) 
                    ? "" 
                    : string.Format("/ClientBin/Images/{0}", new FileInfo(a.Image).Name),
                aaTracks = GetTrackJson(a.TracksOrderedByNumber)
            }).ToList();
        }

        // AJAX: /Player/AddAlbum
        public ActionResult AddAlbum(string artistId, string albumId)
        {
            try
            {
                var album =
                    MusicLibrary.Artists.Where(a => a.Id == new Guid(artistId)).First().Albums.Where(
                        a => a.Id == new Guid(albumId)).First();

                foreach (var track in album.TracksOrderedByNumber)
                {
                    AddSong(track.Id.ToString(), track.Path);
                }

                return Json(new
                {
                    sMessage = string.Format("Added Album!")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.InnerException));
            }
        }

        // AJAX: /Player/AddSong
        public ActionResult AddSong(string id, string path)
        {
            try
            {  
                var addSong = true;

                for (var i = 0; i < Player.currentPlaylist.count; i++)
                {
                    var item = Player.currentPlaylist.get_Item(i);
                    if (string.Equals(item.sourceURL, path, StringComparison.OrdinalIgnoreCase))
                    {
                        addSong = false;
                        break;
                    }
                }

                if (addSong)
                {
                    var media = Player.newMedia(HttpUtility.UrlDecode(path));
                    Player.currentPlaylist.appendItem(media);

                    if (!Player.playState.Equals(WMPLib.WMPPlayState.wmppsPlaying))
                    {
                        Player.CurrentItemChange += (Player_CurrentItemChange);
                        Player.PlayStateChange += (Player_PlayStateChange);                      
                        Player.MediaError += (Player_MediaError);
                        Player.controls.play();
                    }
                } 

                return Json(new
                {
                    sMessage = string.Format("Added '{0}' !", path)  
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.InnerException));
            }
        }

        // AJAX: /Player/RemoveSong
        public ActionResult RemoveSong(string id, string path)
        {
            try
            {
                var mediaItems = new List<IWMPMedia>();
                for (var i = 0; i < Player.currentPlaylist.count; i++)
                {
                    var item = Player.currentPlaylist.get_Item(i);

                    if (string.Equals(item.sourceURL, path, StringComparison.OrdinalIgnoreCase))
                    {
                        mediaItems.Add(item);
                        break;
                    }
                }

                foreach (var mediaItem in mediaItems)
                {
                    Player.currentPlaylist.removeItem(mediaItem);
                }

                return Json(new
                {
                    sMessage = string.Format("Removed '{0}' !", path)  
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, string.Format("{0}: {1}", ex.Message, ex.InnerException));
            }
        }

        void Player_PlayStateChange(int newState)
        {
            switch (newState)
            {
                case 1:
                    // Stopped
                    var mediaItems = new List<IWMPMedia>();
                    for (var i = 0; i < Player.currentPlaylist.count; i++)
                    {
                        var item = Player.currentPlaylist.get_Item(i);
                        mediaItems.Add(item);
                    }
                    foreach(var mediaItem in mediaItems)
                    {
                        Player.currentPlaylist.removeItem(mediaItem);
                    }
                    break;
                case 2:
                    // Paused
                    break;
                case 3:
                    // Playing
                    break;
            }
        }

        private MusicLibraryModel GetMusicLibrary(string path)
        {
            using (var streamReader = new StreamReader(path))
            {
                return (MusicLibraryModel)new XmlSerializer(typeof(MusicLibraryModel)).Deserialize(streamReader);
            }
        }

        void Player_CurrentItemChange(object pdispMedia)
        {
            var mediaItems = new List<IWMPMedia>();
            for (var i = 0; i < Player.currentPlaylist.count; i++)
            {
                var item = Player.currentPlaylist.get_Item(i);

                if (string.Equals(item.sourceURL, Player.currentMedia.sourceURL, StringComparison.OrdinalIgnoreCase))
                    break;

                mediaItems.Add(item);
            }

            foreach (var mediaItem in mediaItems)
            {
                Player.currentPlaylist.removeItem(mediaItem);
            }
        }

        private void Player_MediaError(object pMediaObject)
        {
            // Log errors
        }
    }
}
