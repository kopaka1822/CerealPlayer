using System;
using System.Windows.Input;

namespace CerealPlayer.Commands
{
    public class ToggleFullscreenCommand : ICommand
    {
        private readonly Models.Models models;

        public ToggleFullscreenCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            models.Display.Fullscreen = !models.Display.Fullscreen;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}