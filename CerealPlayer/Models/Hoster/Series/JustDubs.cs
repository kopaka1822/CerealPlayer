using System;
using System.Globalization;
using System.Linq;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Series
{
    public class JustDubs : IVideoHoster
    {
        private readonly CultureInfo culture = new CultureInfo("en-US");
        private readonly Models models;

        public JustDubs(Models models)
        {
            this.models = models;
        }

        public string Name => "JustDubs";

        public bool IsFileHoster => false;

        public bool Supports(string website)
        {
            return website.Contains("http://justdubsanime.net");
        }

        public EpisodeInfo GetInfo(string website)
        {
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine series title");
            var episode = website.Substring(idx + 1);

            // cut out the number part (everything is connected by "-")
            var parts = episode.Split('-');
            if (parts.Length == 0) throw new Exception(website + " has an empty series title");

            var episodeTitle = StringUtil.Reduce(parts, " ");

            if (!Int32.TryParse(parts.Last(), NumberStyles.Integer, culture, out var episodeNum))
            {
                // has only one episode
                return new EpisodeInfo
                {
                    SeriesTitle = episodeTitle,
                    EpisodeTitle = episodeTitle,
                    EpisodeNumber = 0
                };
            }

            var lastPartIndex = parts.Length - 1;
            if (lastPartIndex > 2)
            {
                // most links have the format: title-episode-1
                if (parts[lastPartIndex - 1] == "episode")
                    lastPartIndex--;
            }

            var seriesTitle = StringUtil.Reduce(parts, " ", 0, lastPartIndex);

            return new EpisodeInfo
            {
                SeriesTitle = seriesTitle,
                EpisodeTitle = episodeTitle,
                EpisodeNumber = episodeNum
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            return new RecursiveHosterDownloadTask(models, parent, website, false);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            var nextWebsite = HosterUtil.IncrementLastNumber(website);
            return new TestWebsiteExistsTask(models, parent, nextWebsite, this);
        }

        public string FindCompatibleLink(string websiteSource)
        {
            // TODO add justdubsanime.net thing
            return null;
        }
    }
}
