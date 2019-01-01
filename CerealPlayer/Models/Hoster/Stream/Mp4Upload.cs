using System;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Stream
{
    public class Mp4Upload : IVideoHoster
    {
        private readonly Models models;

        public Mp4Upload(Models models)
        {
            this.models = models;
        }

        public bool Supports(string website)
        {
            return website.Contains("mp4upload.com");
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
            return new SearchVideoLinkTask(models, parent, website, ".mp4\"", true);
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
