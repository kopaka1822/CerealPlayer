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
    public class WatchAnime : IVideoHoster
    {
        private readonly Models models;
        private readonly CultureInfo culture = new CultureInfo("en-US");

        public WatchAnime(Models models)
        {
            this.models = models;
        }

        public string Name => "WatchAnime";
        public bool IsFileHoster => false;

        public bool Supports(string website)
        {
            return website.Contains("watchanime.info/");
        }

        /// <summary>
        ///     verifies that the link contains the "series" specifier (and removes the hoster)
        ///     with hoster: ... watchanime.info/episode/title-episode-number/?server=rapidvideo
        ///     without hoster: ...watchanime.info/episode/title-episode-number
        /// </summary>
        /// <param name="website"></param>
        /// <param name="parts"></param>
        /// <returns>parts without hoster</returns>
        internal static string[] VerifyLinkParts(string website, string[] parts)
        {
            // watchanime.info/episode/title-episode-number
            // watchanime.info/episode/title-episode-number/?server=rapidvideo

            if(parts.Length < 3)
                throw new Exception(website + " link too short");

            if (parts[parts.Length - 2] != "episode")
            {
                // maybe hoster was selected?
                var newParts = new string[parts.Length - 1];
                for (var i = 0; i < newParts.Length; ++i)
                    newParts[i] = parts[i];

                parts = newParts;

                if (parts.Length < 3)
                    throw new Exception(website + " link too short");

                if(parts[parts.Length - 2] != "episode")
                    throw new Exception(website + " requires link to an episode (and not the series info)");
            }

            return parts;
        }

        public EpisodeInfo GetInfo(string website)
        {
            // watchanime.info/episode/title-episode-number
            var parts = VerifyLinkParts(website, website.Split('/'));

            var titleParts = parts.Last().Split('-');
            if(titleParts.Length == 0)
                throw new Exception(website + " has empty series title");

            if (!int.TryParse(titleParts.Last(), NumberStyles.Integer, culture, out var number))
            {
                // one episode series?
                var title = StringUtil.Reduce(titleParts, " ");
                return new EpisodeInfo
                {
                    SeriesTitle = title,
                    EpisodeTitle = title,
                    EpisodeNumber = 0
                };
            }

            var lastPartIndex = titleParts.Length - 1;
            if (lastPartIndex > 2)
            {
                // most links have the format: title-episode-1
                if (titleParts[lastPartIndex - 1] == "episode")
                    lastPartIndex--;
            }

            var seriesTitle = StringUtil.Reduce(titleParts, " ", 0, lastPartIndex);

            return new EpisodeInfo
            {
                SeriesTitle = seriesTitle,
                EpisodeTitle = "Episode " + number,
                EpisodeNumber = number
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            return new RevealHosterTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            var nextWebsite = HosterUtil.IncrementLastNumber(website);
            return new TestWebsiteNot404(models, parent, nextWebsite, this, "<section class=\"error-404 not-found\">", true);
            //return new TestWebsiteAndHosterExists(models, parent, nextWebsite, this, true);
        }

        public string FindCompatibleLink(string websiteSource)
        {
            return null;
        }

        public class RevealHosterTask : ISubTask
        {
            private readonly Models models;
            private readonly VideoTaskModel parent;
            private readonly string website;

            public RevealHosterTask(Models models, VideoTaskModel parent, string website)
            {
                this.models = models;
                this.parent = parent;

                // cleanup website link
                // remove hoster:
                // watchanime.info/episode/title-episode-number/?server=rapidvideo
                // or trailing slash
                // watchanime.info/episode/title-episode-number/

                website = website.TrimEnd('/');

                var idx = website.LastIndexOf('/');
                if (website[idx + 1] == '?')
                    // remove last part
                    website = website.Substring(0, idx);


                this.website = website;
            }

            public async void Start()
            {
                try
                {
                    // get source
                    parent.Description = "resolving " + website;
                    var source = await models.Web.Html.GetJsAsynch(website);

                    // hoster are listed in a script with const SERVER_NAME = 'name'
                    var names = new HashSet<string>();

                    var idx = 0;

                    while (true)
                    {
                        idx = source.IndexOf("const SERVER_", idx, StringComparison.OrdinalIgnoreCase);
                        if (idx < 0) break;

                        // search string names within ' '
                        var startIdx = StringUtil.SkipUntil(source, idx, '\'');
                        var endIdx = StringUtil.SkipUntil(source, startIdx + 1, '\'');
                        var name = source.Substring(startIdx + 1, endIdx - startIdx - 1);
                        names.Add(name);
                        idx = endIdx;
                    }

                    if(names.Count == 0)
                        throw new Exception(website + " no video hoster found");

                    // find links on those websites
                    var compatibleHoster = new List<VideoHosterModel.WebsiteHosterPair>();
                    foreach (var name in names)
                    {
                        try
                        {
                            var reqLink = website + "/?server=" + name;
                            parent.Description = "resolving " + reqLink;
                            var reqSource = await models.Web.Html.GetJsAsynch(reqLink);

                            var hoster = await models.Web.VideoHoster.GetHosterFromSourceAsynch(reqLink, reqSource);
                            compatibleHoster.Add(hoster);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    if (compatibleHoster.Count == 0)
                        throw new Exception(website + " no compatible hoster found");

                    var finalHoster = models.Web.VideoHoster.GetPreferredHoster(compatibleHoster);

                    parent.SetNewSubTask(finalHoster.GetDownloadTask(parent));
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
    }
}
