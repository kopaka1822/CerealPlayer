using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Commands.Playlist.Video;
using CerealPlayer.Models.Hoster;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using Newtonsoft.Json;

namespace CerealPlayer.Models.Playlist
{
    public class VideoModel
    {
        public class SaveData
        {
            public string Website { get; set; }
            public string Name { get; set; }
            public int Number { get; set; }
            public string Extension { get; set; }
            public bool IsDownloaded { get; set; }
        }

        public VideoModel(Models models, string initialWebsite, PlaylistModel parent, IVideoHoster hoster)
        {
            InitialWebsite = initialWebsite;
            Parent = parent;
            DeleteTask = CreateDeleteTask(models);

            // try to find an appropriate downloader
            var info = hoster.GetInfo(initialWebsite);
            Name = info.EpisodeTitle;
            Number = info.EpisodeNumber;

            // create download task
            var task = new VideoTaskModel(this);
            task.SetNewSubTask(hoster.GetDownloadTask(task, initialWebsite));
            DownloadTask = task;
        }

        public VideoModel(Models models, PlaylistModel parent, IVideoHoster hoster, SaveData data)
        {
            InitialWebsite = data.Website;
            Parent = parent;
            DeleteTask = CreateDeleteTask(models);
            Name = data.Name;
            Number = data.Number;
            Extension = data.Extension;

            // create download task
            var task = new VideoTaskModel(this);
            if (data.IsDownloaded && File.Exists(FileLocation))
            {
                task.Description = "";
            }
            else
            {
                task.SetNewSubTask(hoster.GetDownloadTask(task, data.Website));
            }

            DownloadTask = task;
        }

        public PlaylistModel Parent { get; }

        public string InitialWebsite { get; }

        public string Name { get; }

        public int Number { get; }

        public string Extension { get; set; } = ".mp4";

        public string FileLocation => Parent.Directory + "/" + Name + Extension;

        [NotNull]
        public VideoTaskModel DownloadTask { get; }

        /// <summary>
        /// task that will be used for the delayed delete (if delete episode after watching is true)
        /// </summary>
        [NotNull]
        public TaskModel DeleteTask { get; }

        public SaveData GetSaveData()
        {
            return new SaveData
            {
                Website = InitialWebsite,
                Name = Name,
                Number = Number,
                Extension = Extension,
                IsDownloaded = DownloadTask.Status == TaskModel.TaskStatus.Finished
            };
        }

        private TaskModel CreateDeleteTask(Models models)
        {
            var task = new TaskModel(1, TimeSpan.Zero);
            var deleteCommand = new DeleteVideoCommand(models, this, false);
            task.SetNewSubTask(new DelayedCommandTask(task, deleteCommand, 10, "deleting video in "));
            // set this task to failed (don't delete the video yet)
            task.Stop();
            return task;
        }

    }
}
