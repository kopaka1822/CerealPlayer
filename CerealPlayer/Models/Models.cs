using CerealPlayer.Models.Player;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Web;

namespace CerealPlayer.Models
{
    public class Models
    {
        public Models(MainWindow window)
        {
            Settings = SettingsModel.Load();
            App = new AppModel(window);
            Playlists = new PlaylistsModel();
            Display = new DisplayModel();
            Player = new PlayerModel();
            Web = new WebModel(this);
        }

        public AppModel App { get; }
        public WebModel Web { get; }
        public PlaylistsModel Playlists { get; }
        public DisplayModel Display { get; }
        public PlayerModel Player { get; }
        public SettingsModel Settings { get; }

        public void Dispose()
        {
            Settings.Save();
            Web.Dispose();
        }
    }
}