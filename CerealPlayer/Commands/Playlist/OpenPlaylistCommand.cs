using System;
using System.Windows.Input;
using CerealPlayer.ViewModels.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.Commands.Playlist
{
    public class OpenPlaylistCommand : ICommand
    {
        private readonly Models.Models models;

        public OpenPlaylistCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var view = new PlaylistsPreviewView();
            view.DataContext = new PlaylistsPreviewViewModel(models, view);

            models.App.ShowDialog(view);
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
