using System;
using System.Windows.Input;
using CerealPlayer.ViewModels.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.Commands.Playlist
{
    public class OpenPlaylistsCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistsPreviewViewModel viewModel;

        public OpenPlaylistsCommand(Models.Models models)
        {
            this.models = models;
            // save the viewmodel
            viewModel = new PlaylistsPreviewViewModel(models);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var view = new PlaylistsPreviewView
            {
                DataContext = viewModel
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
