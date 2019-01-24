using System;
using System.Globalization;
using System.Linq;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Series
{
    public class MasterAnime : IVideoHoster
    {
        private readonly CultureInfo culture = new CultureInfo("en-US");
        private readonly Models models;

        public MasterAnime(Models models)
        {
            this.models = models;
        }

        public string Name => "MasterAnime";
        public bool IsFileHoster => false;

        public bool Supports(string website)
        {
            return website.Contains("www.masterani.me/");
        }

        public EpisodeInfo GetInfo(string website)
        {
            // www..../watch/id-series-title/number
            var parts = website.Split('/');
            if (parts.Length < 3)
                throw new Exception(website + " link too short");

            if (parts[parts.Length - 3] != "watch")
                throw new Exception(website + " requires link to an episode (and not the series info)");

            // id-series-title (cut out id)
            var seriesParts = parts[parts.Length - 2].Split('-');
            var seriesTitle = StringUtil.Reduce(seriesParts, " ", 1, seriesParts.Length);

            if (!int.TryParse(parts.Last(), NumberStyles.Integer, culture, out var episodeNum))
            {
                episodeNum = 0;
            }

            return new EpisodeInfo
            {
                SeriesTitle = seriesTitle,
                // todo episode title is listed on the overview page of the series
                EpisodeTitle = seriesTitle + " " + episodeNum,
                EpisodeNumber = episodeNum
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            // TODO discover more than the primary hoster (javascript event in player)
            return new RecursiveHosterDownloadTask(models, parent, website, true);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            var nextWebsite = HosterUtil.IncrementLastNumber(website);
            return new TestWebsiteAndHosterExists(models, parent, nextWebsite, this, true);
        }

        public string FindCompatibleLink(string websiteSource)
        {
            return null;
        }
    }
}