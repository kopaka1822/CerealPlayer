using System;
using System.Net;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;

namespace CerealPlayer.Models.Hoster.Tasks
{
    public class DefaultVideoDownloader : ISubTask
    {
        private Models models;
        private readonly VideoTaskModel parent;
        private readonly string link;
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

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            parent.Progress = e.ProgressPercentage;
        }

        public void Stop()
        {
            webClient?.CancelAsync();
        }
    }
}
