using System;
using System.Windows.Input;
using CerealPlayer.ViewModels.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.Commands.Playlist.NonLoaded
{
    public class NewPlaylistCommand : ICommand
    {
        private readonly Models.Models models;

        public NewPlaylistCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var view = new PlaylistCreationView
            {
                DataContext = new PlaylistCreationViewModel(models)
            };

            models.App.ShowDialog(view);
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
