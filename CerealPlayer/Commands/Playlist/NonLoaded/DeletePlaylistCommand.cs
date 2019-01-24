using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CerealPlayer.Commands.Playlist.NonLoaded
{
    public class DeletePlaylistCommand : ICommand
    {
        private readonly string directory;
        private readonly Models.Models models;

        public DeletePlaylistCommand(Models.Models models, string directory)
        {
            this.models = models;
            this.directory = directory;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (MessageBox.Show(models.App.TopmostWindow, $"Do you want to delete \"{Path.GetFileName(directory)}\"?",
                    "Delete Playlist",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                try
                {
                    // delete the folder
                    Directory.Delete(directory, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show(models.App.TopmostWindow, "Could not delete all files. " + e.Message, "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                models.Playlists.OnDirectoryRefresh();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}