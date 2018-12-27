using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CerealPlayer.Commands
{
    /// <summary>
    /// sets the result of the topmost window/dialog
    /// </summary>
    public class SetDialogResultCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly bool result;

        public SetDialogResultCommand(Models.Models models, bool result)
        {
            this.result = result;
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            models.App.TopmostWindow.DialogResult = result;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
