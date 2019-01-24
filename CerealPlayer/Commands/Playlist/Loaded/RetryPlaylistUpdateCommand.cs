using System;
using System.ComponentModel;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Commands.Playlist.Loaded
{
    public class RetryPlaylistUpdateCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistModel playlist;

        public RetryPlaylistUpdateCommand(Models.Models models, PlaylistModel playlist)
        {
            this.models = models;
            this.playlist = playlist;
            this.playlist.DownloadPlaylistTask.PropertyChanged += PlaylistTaskOnPropertyChanged;
            this.playlist.NextEpisodeTask.PropertyChanged += PlaylistTaskOnPropertyChanged;
        }

        public bool CanExecute(object parameter)
        {
            return playlist.DownloadPlaylistTask.Status == TaskModel.TaskStatus.Failed ||
                   playlist.NextEpisodeTask.Status == TaskModel.TaskStatus.Failed;
        }

        public void Execute(object parameter)
        {
            // resume tasks that failed
            if (playlist.NextEpisodeTask.Status == TaskModel.TaskStatus.Failed)
                playlist.NextEpisodeTask.ResetStatus();

            if (playlist.DownloadPlaylistTask.Status == TaskModel.TaskStatus.Failed)
                playlist.DownloadPlaylistTask.ResetStatus();
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