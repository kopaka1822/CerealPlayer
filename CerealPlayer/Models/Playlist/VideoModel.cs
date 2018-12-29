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
            public int Number { get; set; }
            public string Extension { get; set; }
            public bool IsDownloaded { get; set; }
        }

        public VideoModel(Models models, string initialWebsite, PlaylistModel parent, IVideoHoster hoster)
        {
            InitialWebsite = initialWebsite;
            this.Parent = parent;

            // try to find an appropriate downloader
            var info = hoster.GetInfo(initialWebsite);
            this.Name = info.EpisodeTitle;
            this.Number = info.EpisodeNumber;

            // create download task
            var task = new VideoTaskModel(this);
            task.SetNewSubTask(hoster.GetDownloadTask(task, initialWebsite));
            this.DownloadTask = task;
        }

        public VideoModel(Models models, PlaylistModel parent, IVideoHoster hoster, SaveData data)
        {
            InitialWebsite = data.Website;
            this.Parent = parent;
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

            this.DownloadTask = task;
        }

        public PlaylistModel Parent { get; }

        public string InitialWebsite { get; }

        public string Name { get; }

        public int Number { get; }

        public string Extension { get; set; } = ".mp4";

        public string FileLocation => Parent.Directory + "/" + Name + Extension;

        [NotNull]
        public VideoTaskModel DownloadTask { get; }

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
    }
}
