using System;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Tasks
{
    /// <summary>
    /// uses youtube-dl to resolve the link
    /// </summary>
    public class YoutubeDlLinkTask : ISubTask
    {
        private readonly Models models;
        private readonly VideoTaskModel parent;
        private readonly string website;

        public YoutubeDlLinkTask(Models models, VideoTaskModel parent, string website)
        {
            this.models = models;
            this.parent = parent;
            this.website = website;
        }

        public async void Start()
        {
            try
            {
                parent.Description = "resolving " + website;
                var videoLink = await YoutubeDl.GetDownloadUrlAsynch(website);

                parent.SetNewSubTask(new DefaultVideoDownloader(models, parent, videoLink));
            }
            catch (Exception e)
            {
                parent.SetError(e.Message);
            }
        }

        public void Stop()
        {

        }
    }
}
