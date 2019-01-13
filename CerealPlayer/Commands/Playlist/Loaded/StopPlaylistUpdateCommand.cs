using System;
using System.ComponentModel;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Commands.Playlist.Loaded
{
    public class StopPlaylistUpdateCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistModel playlist;

        public StopPlaylistUpdateCommand(Models.Models models, PlaylistModel playlist)
        {
            this.models = models;
            this.playlist = playlist;
            this.playlist.DownloadPlaylistTask.PropertyChanged += PlaylistTaskOnPropertyChanged;
            this.playlist.NextEpisodeTask.PropertyChanged += PlaylistTaskOnPropertyChanged;
        }

        public bool CanExecute(object parameter)
        {
            return playlist.DownloadPlaylistTask.ReadyOrRunning || playlist.NextEpisodeTask.ReadyOrRunning;
        }

        public void Execute(object parameter)
        {
            // stop tasks that are running
            if (playlist.DownloadPlaylistTask.ReadyOrRunning)
                playlist.DownloadPlaylistTask.Stop();

            if (playlist.NextEpisodeTask.ReadyOrRunning)
                playlist.NextEpisodeTask.Stop();
        }

        public event EventHandler CanExecuteChanged;

        private void PlaylistTaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(TaskModel.Status)) OnCanExecuteChanged();
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}