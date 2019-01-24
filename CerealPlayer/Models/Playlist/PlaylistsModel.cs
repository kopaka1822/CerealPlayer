using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CerealPlayer.Annotations;

namespace CerealPlayer.Models.Playlist
{
    public class PlaylistsModel : INotifyPropertyChanged
    {
        private PlaylistModel activePlaylist = null;
        public ObservableCollection<PlaylistModel> List { get; } = new ObservableCollection<PlaylistModel>();

        public PlaylistModel ActivePlaylist
        {
            get => activePlaylist;
            set
            {
                if (ReferenceEquals(value, activePlaylist)) return;
                // should be in the playlists list
                Debug.Assert(value == null || List.Contains(value));

                activePlaylist = value;
                OnPropertyChanged(nameof(ActivePlaylist));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     this event should be raised if anything in the playlist/ directory changed and the contents should be reloaded
        /// </summary>
        public event EventHandler DirectoryRefresh;

        /// <summary>
        ///     raises the DirectoryRefresh event
        /// </summary>
        public virtual void OnDirectoryRefresh()
        {
            DirectoryRefresh?.Invoke(this, EventArgs.Empty);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}