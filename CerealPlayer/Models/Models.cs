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
    public class Models : INotifyPropertyChanged
    {
        public AppModel App { get; }
        public WebModel Web { get; }

        private PlaylistModel playlist = null;

        /// <summary>
        /// active playlist (may be null)
        /// </summary>
        public PlaylistModel Playlist
        {
            get => playlist;
            set
            {
                if (ReferenceEquals(playlist, value)) return;
                playlist = value;
                OnPropertyChanged(nameof(Playlist));
            }
        }

        public Models(MainWindow window)
        {
            App = new AppModel(window);
            Web = new WebModel(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
