using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Hoster;
using CerealPlayer.Models.Player;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Web;

namespace CerealPlayer.Models
{
    public class Models
    {
        public AppModel App { get; }
        public WebModel Web { get; }
        public PlaylistsModel Playlists { get; }
        public DisplayModel Display { get; }
        public PlayerModel Player { get; }
        public SettingsModel Settings { get; }

        public Models(MainWindow window)
        {
            Settings = new SettingsModel();
            App = new AppModel(window);
            Playlists = new PlaylistsModel();
            Display = new DisplayModel();
            Player = new PlayerModel();
            Web = new WebModel(this);
        }

        public void Dispose()
        {
            Settings.Save();
            Web.Dispose();
        }
    }
}
