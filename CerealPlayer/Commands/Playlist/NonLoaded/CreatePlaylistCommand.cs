using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;
using CerealPlayer.ViewModels.Playlist;

namespace CerealPlayer.Commands.Playlist.NonLoaded
{
    public class CreatePlaylistCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistCreationViewModel viewModel;

        public CreatePlaylistCommand(Models.Models models, PlaylistCreationViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.models = models;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        public bool CanExecute(object parameter)
        {
            return viewModel.Address.Length > 0;
        }

        public void Execute(object parameter)
        {
            try
            {
                var playlist = new PlaylistModel(models, viewModel.Address);
                models.Playlists.List.Add(playlist);

                if (viewModel.Play)
                    models.Playlists.ActivePlaylist = playlist;

                models.App.TopmostWindow.DialogResult = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(models.App.TopmostWindow, e.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistCreationViewModel.Address):
                    OnCanExecuteChanged();
                    break;
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}