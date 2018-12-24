using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;
using CerealPlayer.ViewModels.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.Commands.Playlist
{
    public class CreatePlaylistCommand : ICommand
    {
        private readonly PlaylistCreationViewModel viewModel;
        private readonly Window view;
        private readonly Models.Models models;

        public CreatePlaylistCommand(Models.Models models, PlaylistCreationViewModel viewModel, Window view)
        {
            this.viewModel = viewModel;
            this.view = view;
            this.models = models;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistCreationViewModel.Address):
                    OnCanExecuteChanged();
                    break;
            }
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
                models.Playlist = playlist;
                view.DialogResult = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(models.App.Window, e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
