using System;
using System.ComponentModel;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist.Loaded
{
    public class SetActivePlaylistCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistModel model;

        public SetActivePlaylistCommand(Models.Models models, PlaylistModel model)
        {
            this.models = models;
            this.model = model;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName == nameof(PlaylistsModel.ActivePlaylist)) OnCanExecuteChanged();
        }


        public bool CanExecute(object parameter)
        {
            return !ReferenceEquals(models.Playlists.ActivePlaylist, model);
        }

        public void Execute(object parameter)
        {
            models.Playlists.ActivePlaylist = model;
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
