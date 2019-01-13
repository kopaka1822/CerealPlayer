using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Series
{
    public class RoosterTeeth : IVideoHoster
    {
        private readonly CultureInfo culture = new CultureInfo("en-US");
        private readonly Models models;

        public RoosterTeeth(Models models)
        {
            this.models = models;
        }

        public string Name => "RoosterTeeth";
        public bool IsFileHoster => false;

        public bool Supports(string website)
        {
            return website.Contains("roosterteeth.com");
        }

        public EpisodeInfo GetInfo(string website)
        {
            // roosterteeth.com/episode/series-title-number
            var parts = website.Split('/');
            if(parts.Length < 2) throw new Exception(website + " link too short");

            // assert episode
            if (parts[parts.Length - 2] != "episode")
                throw new Exception(website + " required link to an episode");

            // series-title-number
            var seriesParts = parts.Last().Split('-');
            if(seriesParts.Length == 0)
                throw new Exception(website + " has an empty series title");

            // retries website for episode title
            //var source = System.Threading.Tasks.Task.Run(() => models.Web.Html.GetJsAsynch(website)).Result;
            //var episodeBeginString = "<span class=\"video-details__title\">";
            //var idx = source.IndexOf(episodeBeginString, StringComparison.Ordinal);

            string episodeTitle = null;
            //if (idx >= 0)
            //{
            //    var startIdx = StringUtil.SkipUntil(source, idx, '>') + 1;
            //    var endIdx = StringUtil.SkipUntil(source, startIdx, '<');
            //    episodeTitle = source.Substring(startIdx, endIdx - startIdx);
            //    // is a '-' in the title?
            //    idx = episodeTitle.IndexOf('-');
            //    if (idx >= 0)
            //    {
            //        idx = StringUtil.SkipWhitespace(episodeTitle, idx + 1);
            //        if(idx < episodeTitle.Length - 1)
            //            episodeTitle = episodeTitle.Substring(idx);
            //    }
            //}

            // is the last one a number?
            if (!int.TryParse(seriesParts.Last(), NumberStyles.Integer, culture, out var number))
            {
                var title = StringUtil.Reduce(seriesParts, " ");
                // only one episdoe
                return new EpisodeInfo
                {
                    SeriesTitle = title,
                    EpisodeTitle = episodeTitle ?? title,
                    EpisodeNumber = 0
                };
            }

            var seriesTitle = StringUtil.Reduce(seriesParts, " ", 0, seriesParts.Length - 1);

            return new EpisodeInfo
            {
                SeriesTitle = seriesTitle,
                EpisodeTitle = episodeTitle ?? seriesTitle,
                EpisodeNumber = number
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            return new YoutubeDlLinkTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            var nextWebsite = HosterUtil.IncrementLastNumber(website);
            return new TestWebsiteNot404(models, parent, nextWebsite, this, "<div class=\"empty-man__heading\">404 Page not found</div>");
        }

        public string FindCompatibleLink(string websiteSource)
        {
            return null;
        }
    }
}
