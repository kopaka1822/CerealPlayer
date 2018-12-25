using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;

namespace CerealPlayer.Models.Playlist
{
    public class PlaylistsModel : INotifyPropertyChanged
    {
        public ObservableCollection<PlaylistModel> List { get; } = new ObservableCollection<PlaylistModel>();

        private PlaylistModel activePlaylist = null;

        public PlaylistModel ActivePlaylist
        {
            get => activePlaylist;
            set
            {
                if (ReferenceEquals(value, activePlaylist)) return;
                // should be in the playlists list
                Debug.Assert(List.Contains(value));

                activePlaylist = value;
                OnPropertyChanged(nameof(ActivePlaylist));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
