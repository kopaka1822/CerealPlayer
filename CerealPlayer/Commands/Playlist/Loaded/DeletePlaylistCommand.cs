using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist.Loaded
{
    public class DeletePlaylistCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly PlaylistModel playlist;

        public DeletePlaylistCommand(Models.Models models, PlaylistModel playlist)
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
            if (MessageBox.Show(models.App.TopmostWindow, $"Do you want to delete \"{playlist.Name}\"?", "Delete Playlist",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            // remove from active playlist if playing
            if (ReferenceEquals(models.Playlists.ActivePlaylist, playlist))
                models.Playlists.ActivePlaylist = null;

            // remove from playlists
            models.Playlists.List.Remove(playlist);

            try
            {
                // delete the folder
                System.IO.Directory.Delete(playlist.Directory, true);
            }
            catch (Exception e)
            {
                MessageBox.Show(models.App.TopmostWindow, e.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            models.Playlists.OnDirectoryRefresh();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
