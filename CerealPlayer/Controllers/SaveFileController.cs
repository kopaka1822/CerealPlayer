using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Playlist;
using Newtonsoft.Json;

namespace CerealPlayer.Controllers
{
    public class SaveFileController
    {
        private readonly Models.Models models;

        public SaveFileController(Models.Models models)
        {
            this.models = models;
            this.models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
        }

        private void PlaylistOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems)
                {
                    SavePlaylist(item as PlaylistModel);        
                }
            }
        }

        public void SavePlaylist(PlaylistModel playlist)
        {
            var data = JsonConvert.SerializeObject(playlist.GetSaveData());
            File.WriteAllText(playlist.SettingsLocation, data);
        }

        /// <summary>
        /// saves all playlist files
        /// </summary>
        public void Close()
        {
            foreach (var playlistModel in models.Playlists.List)
            {
                SavePlaylist(playlistModel);
            }
        }
    }
}
