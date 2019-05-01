using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;
using CerealPlayer.ViewModels.Settings;
using CerealPlayer.Views.Settings;

namespace CerealPlayer.Commands.Settings
{
    public class ShowPlaylistSettingsCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistModel playlist;

        public ShowPlaylistSettingsCommand(Models.Models models, PlaylistModel playlist)
        {
            this.models = models;
            this.playlist = playlist;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var view = new PlaylistSettingsView();
            var viewModel = new PlaylistSettingsViewModel(models, playlist);
            view.DataContext = viewModel;

            if (models.App.ShowDialog(view) != true) return;

            // apply settings
            playlist.Settings.CustomHosterPreferences = viewModel.HosterList.Items.ToArray();
            playlist.Settings.UseCustomHosterPreferences = viewModel.UseCustomHoster;
            playlist.Save();
        }

        public event EventHandler CanExecuteChanged { add {} remove {} }
    }
}
