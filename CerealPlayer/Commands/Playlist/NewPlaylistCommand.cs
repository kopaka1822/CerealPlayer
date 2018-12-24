using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.ViewModels.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.Commands.Playlist
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
            var view = new PlaylistCreationView();
            view.DataContext = new PlaylistCreationViewModel(models, view);

            view.ShowDialog();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
