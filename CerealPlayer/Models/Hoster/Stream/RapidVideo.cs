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
    public class RapidVideo : IVideoHoster
    {
        private readonly Models models;

        public RapidVideo(Models models)
        {
            this.models = models;
        }

        public bool Supports(string website)
        {
            return website.Contains("rapidvideo.com");
        }

        public EpisodeInfo GetInfo(string website)
        {
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
            return new SearchVideoLinkTask(models, parent, website, ".mp4\" type=\"video", false);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            return null;
        }

        public string FindCompatibleLink(string websiteSource)
        {
            var idx = websiteSource.IndexOf("rapidvideo.com/e/", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            return StringUtil.ReadLink(websiteSource, idx);
        }
    }
}
