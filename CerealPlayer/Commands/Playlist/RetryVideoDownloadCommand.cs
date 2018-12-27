using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Controllers;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Commands.Playlist
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
