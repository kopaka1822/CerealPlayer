using System;
using System.Windows.Input;
using CerealPlayer.ViewModels.Playlist;

namespace CerealPlayer.Commands.Playlist.All
{
    public class UpdateAllPlaylistsCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistsPreviewViewModel viewModel;

        public UpdateAllPlaylistsCommand(Models.Models models, PlaylistsPreviewViewModel viewModel)
        {
            this.models = models;
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.UpdateAllPlaylists();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}