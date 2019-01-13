using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Commands.Playlist.Video
{
    public class DeleteVideoCommand : ICommand
    {
        private readonly bool askUser;
        private readonly Models.Models models;
        private readonly VideoModel video;

        /// <summary>
        /// </summary>
        /// <param name="models"></param>
        /// <param name="video"></param>
        /// <param name="askUser">should the user be asked before deleting the video?</param>
        public DeleteVideoCommand(Models.Models models, VideoModel video, bool askUser)
        {
            this.models = models;
            this.video = video;
            this.askUser = askUser;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (askUser && MessageBox.Show(models.App.TopmostWindow, $"Do you want to delete \"{video.Name}\"?",
                    "Delete Video",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            if (video.Parent.DeleteEpisode(video))
            {
                DeleteAsynch();
            }
        }


        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        private async void DeleteAsynch()
        {
            try
            {
                // wait until the download task aborted
                await Task.Run(() =>
                    SpinWait.SpinUntil(() => video.DownloadTask.Status != TaskModel.TaskStatus.Running));

                // delete the file
                if (File.Exists(video.FileLocation))
                    File.Delete(video.FileLocation);
            }
            catch (Exception e)
            {
                MessageBox.Show(models.App.TopmostWindow, "Could not delete video. " + e.Message, "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}