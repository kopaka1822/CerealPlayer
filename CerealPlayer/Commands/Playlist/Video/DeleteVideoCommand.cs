using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly Models.Models models;
        private readonly VideoModel video;
        private readonly bool askUser;

        /// <summary>
        /// 
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
            if(askUser && MessageBox.Show(models.App.TopmostWindow, $"Do you want to delete \"{video.Name}\"?", "Delete Video", 
                   MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            video.Parent.DeleteEpisode(video);
            DeleteAsynch();
        }

        private async void DeleteAsynch()
        {
            try
            {
                // wait until the download task aborted
                await System.Threading.Tasks.Task.Run(() => 
                    SpinWait.SpinUntil(() => video.DownloadTask.Status != TaskModel.TaskStatus.Running));

                // delete the file
                if (System.IO.File.Exists(video.FileLocation))
                    System.IO.File.Delete(video.FileLocation);
            }
            catch (Exception e)
            {
                MessageBox.Show(models.App.TopmostWindow, "Could not delete video. " + e.Message, "Error", MessageBoxButton.OK,
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
