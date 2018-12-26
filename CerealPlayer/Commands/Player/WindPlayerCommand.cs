using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.ViewModels.Player;

namespace CerealPlayer.Commands.Player
{
    public class WindPlayerCommand : ICommand
    {
        private readonly PlayerViewModel viewModel;
        private readonly TimeSpan time;

        public WindPlayerCommand(PlayerViewModel viewModel, TimeSpan time)
        {
            this.viewModel = viewModel;
            this.time = time;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.Wind(time);
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
