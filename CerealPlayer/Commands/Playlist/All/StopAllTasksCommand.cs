using System;
using System.Windows.Input;

namespace CerealPlayer.Commands.Playlist.All
{
    public class StopAllTasksCommand : ICommand
    {
        private readonly Models.Models models;

        public StopAllTasksCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            foreach (var playlist in models.Playlists.List)
            {
                if (playlist.DownloadPlaylistTask.ReadyOrRunning)
                    playlist.DownloadPlaylistTask.Stop();

                if (playlist.NextEpisodeTask.ReadyOrRunning)
                    playlist.NextEpisodeTask.Stop();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}