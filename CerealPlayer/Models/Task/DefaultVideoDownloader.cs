using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CerealPlayer.Models.Task
{
    public class DefaultVideoDownloader : ISubTask
    {
        private Models models;
        private readonly DownloadTaskModel parent;
        private readonly string link;
        private WebClient webClient = null;

        public DefaultVideoDownloader(Models models, DownloadTaskModel parent, string link)
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
                parent.Status = TaskModel.TaskStatus.Finished;
            }
            catch (Exception e)
            {
                parent.SetError(e.Message);
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            parent.Percentage = e.ProgressPercentage;
        }

        public void Stop()
        {
            webClient?.CancelAsync();
        }
    }
}
