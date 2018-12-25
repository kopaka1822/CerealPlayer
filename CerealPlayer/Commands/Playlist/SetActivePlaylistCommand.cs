using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist
{
    public class SetActivePlaylistCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistModel model;

        public SetActivePlaylistCommand(Models.Models models, PlaylistModel model)
        {
            this.models = models;
            this.model = model;
        }


        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            models.Playlists.ActivePlaylist = model;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
