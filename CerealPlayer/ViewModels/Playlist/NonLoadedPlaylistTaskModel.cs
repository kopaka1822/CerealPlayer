using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Commands.Playlist.NonLoaded;

namespace CerealPlayer.ViewModels.Playlist
{
    public class NonLoadedPlaylistTaskModel : IPlaylistTaskViewModel
    {
        public NonLoadedPlaylistTaskModel(Models.Models models, string directory)
        {
            Name = Path.GetFileName(directory);

            PlayCommand = new UpdatePlaylistDirectoryCommand(models, directory, true);
            RetryCommand = new UpdatePlaylistDirectoryCommand(models, directory, false);
            DeleteCommand = new DeletePlaylistCommand(models, directory);
        }

        public string Name { get; }

        public ICommand PlayCommand { get; }

        public ICommand RetryCommand { get; }

        public ICommand StopCommand => null;

        public ICommand DeleteCommand { get; }

        public Visibility PlayVisibility => Visibility.Visible;

        public Visibility RetryVisibility => Visibility.Visible;

        public Visibility StopVisibility => Visibility.Collapsed;

        public string Status => "";

        public Visibility ProgressVisibility => Visibility.Collapsed;

        public int Progress { get ; set; }
    }
}
