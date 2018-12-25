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
        private readonly PlayerViewModel viewModel;

        public PlayPauseCommand(PlayerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.PlayPause();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
