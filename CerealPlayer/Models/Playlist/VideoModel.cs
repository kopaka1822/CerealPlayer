using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Models.Playlist
{
    public class VideoModel
    {
        private readonly PlaylistModel parent;

        public VideoModel(Models models, string initialWebsite, PlaylistModel parent)
        {
            InitialWebsite = initialWebsite;
            this.parent = parent;

            // try to find an appropriate downloader
            var hoster = models.Web.VideoHoster.GetCompatibleHoster(initialWebsite);
            this.Name = hoster.GetEpisodeTitle(initialWebsite);

            // create download task
            var task = new TaskModel(this);
            task.SetNewSubTask(hoster.GetDownloadTask(task, initialWebsite));
            this.DownloadTask = task;
        }

        public string InitialWebsite { get; }

        public string Name { get; }

        public string Extension { get; set; } = ".mp4";

        public string FileLocation => parent.Directory + "/" + Name + Extension;

        [NotNull]
        public TaskModel DownloadTask { get; }
    }
}
