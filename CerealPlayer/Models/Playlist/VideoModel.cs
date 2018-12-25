using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Hoster;
using CerealPlayer.Models.Task;
using Newtonsoft.Json;

namespace CerealPlayer.Models.Playlist
{
    public class VideoModel
    {
        public class SaveData
        {
            public string Website { get; set; }
            public string Name { get; set; }
            public string Extension { get; set; }
            public bool IsDownloaded { get; set; }
        }

        private readonly PlaylistModel parent;

        public VideoModel(Models models, string initialWebsite, PlaylistModel parent, IVideoHoster hoster)
        {
            InitialWebsite = initialWebsite;
            this.parent = parent;

            // try to find an appropriate downloader
            this.Name = hoster.GetEpisodeTitle(initialWebsite);

            // create download task
            var task = new DownloadTaskModel(this);
            task.SetNewSubTask(hoster.GetDownloadTask(task, initialWebsite));
            task.PropertyChanged += TaskOnPropertyChanged;
            this.DownloadTask = task;
        }

        private void TaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(TaskModel.Status):
                    parent.DownloadStatus = DownloadTask.Status;
                    if (DownloadTask.Status == TaskModel.TaskStatus.Running)
                        parent.DownloadProgress = 0; // reset download progress
                    break;
                case nameof(TaskModel.Description):
                    parent.DownloadDescription = DownloadTask.Description;
                    break;
                case nameof(TaskModel.Percentage):
                    parent.DownloadProgress = DownloadTask.Percentage;
                    break;
            }
        }

        public VideoModel(Models models, PlaylistModel parent, IVideoHoster hoster, SaveData data)
        {
            InitialWebsite = data.Website;
            this.parent = parent;
            Name = data.Name;
            Extension = data.Extension;

            // create download task
            var task = new DownloadTaskModel(this);
            if (data.IsDownloaded && File.Exists(FileLocation))
            {
                task.Status = TaskModel.TaskStatus.Finished;
            }
            else
            {
                task.SetNewSubTask(hoster.GetDownloadTask(task, data.Website));
                task.PropertyChanged += TaskOnPropertyChanged;
            }

            this.DownloadTask = task;
        }

        public string InitialWebsite { get; }

        public string Name { get; }

        public string Extension { get; set; } = ".mp4";

        public string FileLocation => parent.Directory + "/" + Name + Extension;

        [NotNull]
        public DownloadTaskModel DownloadTask { get; }

        public SaveData GetSaveData()
        {
            return new SaveData
            {
                Website = InitialWebsite,
                Name = Name,
                Extension = Extension,
                IsDownloaded = DownloadTask.Status == TaskModel.TaskStatus.Finished
            };
        }
    }
}
