using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;
using CerealPlayer.ViewModels.Player;

namespace CerealPlayer.Commands.Player
{
    public class WindPlayerCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly TimeSpan time;
        private PlaylistModel activePlaylist = null;

        public WindPlayerCommand(Models.Models models, TimeSpan time)
        {
            this.models = models;
            this.time = time;
            Debug.Assert(models.Playlists.ActivePlaylist == null);
            models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged -= ActivePlaylistOnPropertyChanged;
                    }
                    activePlaylist = models.Playlists.ActivePlaylist;
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged += ActivePlaylistOnPropertyChanged;
                    }
                    OnCanExecuteChanged();
                    break;
            }
        }

        private void ActivePlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideo):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public bool CanExecute(object parameter)
        {
            if (models.Playlists.ActivePlaylist == null) return false;
            if (models.Playlists.ActivePlaylist.PlayingVideo == null) return false;
            return true;
        }

        public void Execute(object parameter)
        {
            var oldPos = models.Playlists.ActivePlaylist.PlayingVideoPosition;
            models.Playlists.ActivePlaylist.PlayingVideoPosition = oldPos.Add(time);
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
