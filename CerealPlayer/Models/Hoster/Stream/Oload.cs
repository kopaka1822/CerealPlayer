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
    public class Oload : IVideoHoster
    {
        private readonly Models models;

        public Oload(Models models)
        {
            this.models = models;
        }

        public bool Supports(string website)
        {
            return website.Contains("oload.tv");
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
            return new YoutubeDlLinkTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            return null;
        }

        public string FindCompatibleLink(string websiteSource)
        {
            var idx = websiteSource.IndexOf("oload.tv/embed/", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            // read entire link
            return StringUtil.ReadLink(websiteSource, idx);
        }
    }
}
