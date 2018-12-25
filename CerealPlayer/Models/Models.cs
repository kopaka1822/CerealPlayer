using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Hoster;
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

        public Models(MainWindow window)
        {
            App = new AppModel(window);
            Web = new WebModel(this);
            Playlists = new PlaylistsModel();
            Display = new DisplayModel();
        }
    }
}
