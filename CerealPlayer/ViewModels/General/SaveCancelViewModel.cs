using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Commands;

namespace CerealPlayer.ViewModels.General
{
    /// <summary>
    /// helper view model for the SaveCancelView
    /// => sets the dialog result to true if save is pressed, false otherwise
    /// </summary>
    public class SaveCancelViewModel
    {
        public SaveCancelViewModel(Models.Models models)
        {
            SaveCommand = new SetDialogResultCommand(models, true);
            CancelCommand = new SetDialogResultCommand(models, false);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
    }
}
