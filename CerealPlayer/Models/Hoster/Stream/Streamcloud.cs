using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Stream
{
    public class Streamcloud : IVideoHoster
    {
        private readonly Models models;

        public Streamcloud(Models models)
        {
            this.models = models;
        }

        public bool Supports(string website)
        {
            return website.Contains("http://streamcloud.eu");
        }

        public EpisodeInfo GetInfo(string website)
        {
            // get video id
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine series title");

            var res = website.Substring(idx + 1);

            return new EpisodeInfo
            {
                EpisodeNumber = 0,
                EpisodeTitle = res,
                SeriesTitle = res
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            // TODO use another downloader (default downloader returns 403 forbidden)
            return new YoutubeDlLinkTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            return null;
        }

        public string FindCompatibleLink(string websiteSource)
        {
            var idx = websiteSource.IndexOf("streamcloud.eu/", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            return StringUtil.ReadLink(websiteSource, idx);
        }
    }
}
