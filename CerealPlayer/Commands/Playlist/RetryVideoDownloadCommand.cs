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
        private readonly TaskController ctrl;

        public RetryVideoDownloadCommand(PlaylistModel playlist, TaskController controller)
        {
            this.playlist = playlist;
            this.ctrl = controller;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ctrl.RetryTask(playlist);   
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
