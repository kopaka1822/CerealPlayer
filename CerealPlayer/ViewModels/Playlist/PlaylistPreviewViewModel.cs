using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands.Playlist;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistPreviewViewModel
    {
        public PlaylistPreviewViewModel(Models.Models models, string directory)
        {
            Name = Path.GetFileName(directory);
            UpdateCommand = new UpdatePlaylistDirectoryCommand(models, directory);
        }

        public string Name { get; }

        public ICommand UpdateCommand { get; }
    }
}
