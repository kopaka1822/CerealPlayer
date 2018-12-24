using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CerealPlayer.Commands
{
    public class SetDialogResultCommand : ICommand
    {
        private readonly Window dialog;
        private readonly bool result;

        public SetDialogResultCommand(Window dialog, bool result)
        {
            this.dialog = dialog;
            this.result = result;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            dialog.DialogResult = result;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
