using System;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Stream
{
    public class Mp4Upload : IVideoHoster
    {
        class DownloadTask : ISubTask
        {
            private readonly Models models;
            private readonly VideoTaskModel parent;
            private readonly string website;

            public DownloadTask(Models models, VideoTaskModel parent, string website)
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
                    var source = await models.Web.Html.GetJsAsynch(website);
                    // search .mp4"
                    var subIndex = source.IndexOf(".mp4\"", StringComparison.Ordinal);
                    if (subIndex < 0)
                    {
                        // remove website from cache (did not load correctly)
                        models.Web.Html.RemoveCachedJs(website);
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

        public EpisodeInfo GetInfo(string website)
        {
            // the random string after the website title
            // TODO determine correct title
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine series title");
            var res = website.Substring(idx + 1);

            return new EpisodeInfo
            {
                SeriesTitle = res,
                EpisodeTitle = res,
                EpisodeNumber = 0
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            return new DownloadTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            // does not support next episode
            return null;
        }

        public string FindCompatibleLink(string websiteSource)
        {
            // crawl to find supported links
            var index = websiteSource.IndexOf("mp4upload.com/embed-", StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                // found link
                return StringUtil.ReadLink(websiteSource, index);
            }

            // the link is probably hidden within player.mp4cloud.net
            index = websiteSource.IndexOf("player.mp4cloud.net/mp4upload.php?id=", StringComparison.InvariantCultureIgnoreCase);

            if (index < 0) return null;

            // the embed link is given after id
            index = StringUtil.SkipUntil(websiteSource, index, '=');
            // read id string
            var id = StringUtil.ReadLink(websiteSource, index + 1);

            return "https://www.mp4upload.com/embed-" + id + ".html";
        }
    }
}
