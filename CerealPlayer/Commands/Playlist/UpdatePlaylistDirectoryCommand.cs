using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist
{
    public class UpdatePlaylistDirectoryCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly string directory;

        public UpdatePlaylistDirectoryCommand(Models.Models models, string directory)
        {
            this.directory = directory;
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            // add new playlist model            
            models.Playlists.List.Add(PlaylistModel.LoadFromDirectory(directory, models));
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
