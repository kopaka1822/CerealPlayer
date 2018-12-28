using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.ViewModels.Player;

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
