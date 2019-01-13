using System;
using System.Net;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Models.Hoster.Tasks
{
    public class DefaultVideoDownloader : ISubTask
    {
        private readonly string link;
        private readonly VideoTaskModel parent;
        private Models models;
        private WebClient webClient = null;

        public DefaultVideoDownloader(Models models, VideoTaskModel parent, string link)
        {
            this.models = models;
            this.parent = parent;
            this.link = link;
        }

        public async void Start()
        {
            try
            {
                parent.Description = "downloading file";
                using (webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += DownloadProgressChanged;

                    await webClient.DownloadFileTaskAsync(link, parent.Video.FileLocation);
                }

                parent.Description = "finished download";
                parent.SetFinished();
            }
            catch (Exception e)
            {
                parent.SetError(e.Message);
            }
        }

        public void Stop()
        {
            webClient?.CancelAsync();
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            parent.Progress = e.ProgressPercentage;
        }
    }
}