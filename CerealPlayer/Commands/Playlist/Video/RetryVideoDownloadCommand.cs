using System;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist.Video
{
    public class RetryVideoDownloadCommand : ICommand
    {
        private readonly PlaylistModel playlist;

        public RetryVideoDownloadCommand(PlaylistModel playlist)
        {
            this.playlist = playlist;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            playlist.DownloadPlaylistTask.ResetStatus();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}