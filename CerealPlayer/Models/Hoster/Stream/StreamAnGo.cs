using System;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Stream
{
    public class StreamAnGo : IVideoHoster
    {
        private readonly Models models;

        public StreamAnGo(Models models)
        {
            this.models = models;
        }

        public string Name => "StreaMango";
        public bool IsFileHoster => true;

        public bool Supports(string website)
        {
            return website.Contains("streamango.com");
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
            if(website.Contains("/embed/"))
                return new YoutubeDlLinkTask(models, parent, website);
            
            // this is not the embed link, but the embed link should be on this website
            return new RecursiveHosterDownloadTask(models, parent, website, true);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            return null;
        }

        public string FindCompatibleLink(string websiteSource)
        {
            // embed link
            var idx = websiteSource.IndexOf("streamango.com/embed/", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) return StringUtil.ReadLink(websiteSource, idx);

            // link without embed
            idx = websiteSource.IndexOf("streamango.comicsonline.to/getLinkSimple?file=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) return StringUtil.ReadLink(websiteSource, idx);

            return null;
        }
    }
}