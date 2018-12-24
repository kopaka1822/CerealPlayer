using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster
{
    public class Mp4Upload : IVideoHoster
    {
        class DownloadTask : ISubTask
        {
            private readonly Models models;
            private readonly TaskModel parent;
            private readonly string website;

            public DownloadTask(Models models, TaskModel parent, string website)
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
                    var source = await models.Web.Html.GetAsynch(website);
                    // search .mp4"
                    var subIndex = source.IndexOf(".mp4\"", StringComparison.Ordinal);
                    if (subIndex < 0)
                    {
                        // remove website from cache (did not load correctly)
                        models.Web.Html.RemoveCached(website);
                        throw new Exception($"failed to locate \".mp4\"\" in {website}");
                    }

                    var address = StringUtil.BackwardSubstringUntil(
                                  source,
                                  subIndex - 1,
                                  '\"') + ".mp4";

                    parent.SetNewSubTask(new DefaultVideoDownloader(models, parent, address));
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

        private readonly Models models;

        public Mp4Upload(Models models)
        {
            this.models = models;
        }

        public bool Supports(string website)
        {
            return website.Contains("www.mp4upload.com");
        }

        public string GetSeriesTitle(string website)
        {
            // the random string after the website title
            // TODO determine correct title
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine series title");
            return website.Substring(idx + 1);
        }

        public string GetEpisodeTitle(string website)
        {
            return GetSeriesTitle(website);
        }

        public ISubTask GetDownloadTask(TaskModel parent, string website)
        {
            return new DownloadTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(TaskModel parent, string website)
        {
            // does not support next episode
            return null;
        }
    }
}
