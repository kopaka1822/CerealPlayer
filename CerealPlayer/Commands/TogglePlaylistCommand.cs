using System;
using System.Windows.Input;

namespace CerealPlayer.Commands
{
    public class TogglePlaylistCommand : ICommand
    {
        private readonly Models.Models models;

        public TogglePlaylistCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            models.Display.ShowPlaylist = !models.Display.ShowPlaylist;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}