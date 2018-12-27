using System;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist.NonLoaded
{
    public class UpdatePlaylistDirectoryCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly string directory;
        private readonly bool playPlaylist;

        /// <summary>
        /// Updates (and plays) a playlist that is not yet loaded
        /// </summary>
        /// <param name="models"></param>
        /// <param name="directory">playlist directory</param>
        /// <param name="playPlaylist">indicates if the playlist should be played after loading</param>
        public UpdatePlaylistDirectoryCommand(Models.Models models, string directory, bool playPlaylist)
        {
            this.directory = directory;
            this.playPlaylist = playPlaylist;
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                // add new playlist model            
                var model = PlaylistModel.LoadFromDirectory(directory, models);
                models.Playlists.List.Add(model);
                if (playPlaylist)
                    models.Playlists.ActivePlaylist = model;
            }
            catch (Exception e)
            {
                MessageBox.Show(models.App.TopmostWindow, e.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
