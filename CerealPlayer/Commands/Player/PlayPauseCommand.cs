using System;
using System.Windows.Input;

namespace CerealPlayer.Commands.Player
{
    public class PlayPauseCommand : ICommand
    {
        private readonly Models.Models models;

        public PlayPauseCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            models.Player.IsPausing = !models.Player.IsPausing;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}