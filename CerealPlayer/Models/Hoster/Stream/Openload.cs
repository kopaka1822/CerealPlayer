using System;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Stream
{
    class Openload : IVideoHoster
    {
        private readonly Models models;

        public Openload(Models models)
        {
            this.models = models;
        }

        public string Name => "Openload";
        public bool IsFileHoster => true;

        public bool Supports(string website)
        {
            return website.Contains("openload.co");
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
            var idx = websiteSource.IndexOf("openload.co/embed/", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            // read entire link
            return StringUtil.ReadLink(websiteSource, idx);
        }
    }
}
