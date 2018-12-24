using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CerealPlayer.Models
{
    public class AppModel
    {
        public MainWindow Window { get; }
        public string WorkingDirectory { get; }
        public string PlaylistDirectory { get; }

        public AppModel(MainWindow window)
        {
            this.Window = window;
            WorkingDirectory = Directory.GetCurrentDirectory();
            PlaylistDirectory = WorkingDirectory + "/playlists";
            Directory.CreateDirectory(PlaylistDirectory);
        }
    }
}
