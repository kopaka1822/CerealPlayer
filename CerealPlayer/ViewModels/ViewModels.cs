using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Commands;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.ViewModels.Playlist;

namespace CerealPlayer.ViewModels
{
    public class ViewModels
    {
        // view models
        public PlaylistViewModel Playlist { get; }

        // commands
        public ICommand OpenPlaylistCommand { get; }
        public ICommand NewPlaylistCommand { get; }

        public ViewModels(Models.Models models)
        {
            // view models
            Playlist = new PlaylistViewModel(models);

            // commands
            OpenPlaylistCommand = new OpenPlaylistCommand(models);
            NewPlaylistCommand = new NewPlaylistCommand(models);
        }
    }
}
