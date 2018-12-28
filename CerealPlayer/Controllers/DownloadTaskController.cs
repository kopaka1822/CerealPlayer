using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Controllers
{
    public class DownloadTaskController : TaskControllerBase
    {
        private int prevMaxDownloads;
        private int prevMaxAdvanceDownloads;

        public DownloadTaskController(Models.Models models) : base(models)
        {
            prevMaxDownloads = models.Settings.MaxDownloads;
            prevMaxAdvanceDownloads = models.Settings.MaxAdvanceDownloads;
            models.Settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(SettingsModel.MaxDownloads):
                    if (models.Settings.MaxDownloads > prevMaxDownloads)
                    {
                        StartNewTasks();
                    }

                    prevMaxDownloads = models.Settings.MaxDownloads;
                    break;
                case nameof(SettingsModel.MaxAdvanceDownloads):
                    if (models.Settings.MaxAdvanceDownloads > prevMaxAdvanceDownloads)
                    {
                        StartNewTasks();
                    }

                    prevMaxAdvanceDownloads = models.Settings.MaxDownloads;
                    break;
            }
        }

        protected override TaskModel GetTask(PlaylistModel playlist)
        {
            return playlist.DownloadPlaylistTask;
        }

        protected override bool CanExecuteTasks()
        {
            return NumTaskRunning < models.Settings.MaxDownloads;
        }

        protected override bool CanExecuteTask(PlaylistModel playlist)
        {
            // how many episodes ahead?
            var nextDownloadIndex = 0;
            foreach (var video in playlist.Videos)
            {
                if (video.DownloadTask.Status == TaskModel.TaskStatus.ReadyToStart)
                    break;
                nextDownloadIndex++;
            }

            var episodesAhead = nextDownloadIndex - playlist.PlayingVideoIndex;

            return episodesAhead <= models.Settings.MaxAdvanceDownloads;
        }
    }
}
