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
    public class BurningSeries : IVideoHoster
    {
        // hosters are hidden within the website
        public class RevealHosterTask : ISubTask
        {
            private readonly Models models;
            private readonly VideoTaskModel parent;
            private readonly string website;

            public RevealHosterTask(Models models, VideoTaskModel parent, string website)
            {
                this.models = models;
                this.parent = parent;
                this.website = website;
            }

            public async void Start()
            {
                try
                {
                    // get website in this format:
                    // ...bs.to/serie/series-title/season/number-episode-title/language
                    var linkParts = website.Split('/');

                    linkParts = VerifyLinkParts(website, linkParts);

                    // links to hoster look like this:
                    // serie/series-title/season/number-episode-title/language/hoster
                    var linkStart = StringUtil.Reduce(linkParts, "/", linkParts.Length - 5, linkParts.Length);
                    linkStart += "/";

                    parent.Description = "resolving " + website;
                    var source = await models.Web.Html.GetAsynch(website);

                    // find links with a hoster as last argument
                    var idx = 0;
                    var streamLinks = new HashSet<string>();
                    do
                    {
                        idx = source.IndexOf(linkStart, idx, StringComparison.OrdinalIgnoreCase);
                        if(idx < 0) continue;

                        var link = StringUtil.ReadLink(source, idx);
                        idx++;

                        if (!link.StartsWith("serie")) continue;

                        streamLinks.Add("https://bs.to/" + link);
                    } while (idx > 0);

                    if(streamLinks.Count == 0)
                        throw new Exception(website + " no video hoster found");

                    // find links on those websites
                    parent.Description = "resolving hoster " + website;
                    var compatibleHoster = new List<VideoHosterModel.WebsiteHosterPair>();

                    foreach (var streamLink in streamLinks)
                    {
                        try
                        {
                            var reqSource = await models.Web.Html.GetAsynch(streamLink);

                            // the hoster is hidden within this link: https://bs.to/out/id
                            idx = reqSource.IndexOf("https://bs.to/out/", StringComparison.OrdinalIgnoreCase);
                            if(idx < 0) continue;

                            var videoLink = StringUtil.ReadLink(reqSource, idx);

                            // TODO solve videoLink which is protected by "im not a robot" captcha

                            var hoster = await models.Web.VideoHoster.GetHosterFromSourceAsynch(streamLink, reqSource);
                            compatibleHoster.Add(hoster);
                        }
                        catch (Exception)
                        {
                            //int a = 3;
                        }
                    }

                    /*var requests = new List<Task<string>>();
                    foreach (var link in streamLinks)
                    {
                        requests.Add(models.Web.Html.GetJsAsynch(link));   
                    }

                    // try to find compatible hoster on those pages
                    var hoster = new List<Task<VideoHosterModel.WebsiteHosterPair>>();
                    foreach (var request in requests)
                    {
                        try
                        {
                            var reqSource = await request;
                            // find compatible hoster
                            hoster.Add(models.Web.VideoHoster.GetHosterFromSourceAsynch(website, reqSource));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    foreach (var hosterTask in hoster)
                    {
                        try
                        {
                            var pair = await hosterTask;
                            compatibleHoster.Add(pair);
                        }
                        catch (Exception e)
                        {
                            // ignored
                            int a = 3;
                        }
                    }*/

                    if(compatibleHoster.Count == 0)
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

        public class NextEpisodeTask : ISubTask
        {
            private readonly Models models;
            private readonly NextEpisodeTaskModel parent;
            private readonly string oldWebsite;

            public NextEpisodeTask(Models models, NextEpisodeTaskModel parent, string oldWebsite)
            {
                this.models = models;
                this.parent = parent;
                this.oldWebsite = oldWebsite;
            }

            public async void Start()
            {
                try
                {
                    // get number from website
                    // ...bs.to/serie/series-title/season/number-episode-title/language
                    var linkParts = oldWebsite.Split('/');

                    linkParts = VerifyLinkParts(oldWebsite, linkParts);

                    var epParts = linkParts[linkParts.Length - 2].Split('-');
                    if(epParts.Length == 0)
                        throw new Exception(oldWebsite + " empty episode title");

                    var oldNum = Int32.Parse(epParts[0], NumberStyles.Integer);

                    parent.Description = "resolving " + oldWebsite;
                    var source = await models.Web.Html.GetAsynch(oldWebsite);

                    // relative links on website look like:
                    // serie/series-title/season/number-episode-title/language
                    // search for serie/series-title/season/number
                    var nextLinkStart = StringUtil.Reduce(linkParts, "/", linkParts.Length - 5, linkParts.Length - 2);
                    nextLinkStart += "/" + (oldNum + 1).ToString();

                    var idx = source.IndexOf(nextLinkStart, StringComparison.OrdinalIgnoreCase);
                    if(idx < 0)
                        throw new Exception(""); // episode does not exist

                    // read link
                    var nextLink = StringUtil.ReadLink(source, idx);
                    if (!nextLink.StartsWith("serie"))
                        throw new Exception(nextLink + " does not begin with \"serie\"");

                    // add bs.to domain
                    nextLink = "https://bs.to/" + nextLink;

                    parent.Playlist.AddNextEpisode(nextLink);
                    parent.SetNewSubTask(new NextEpisodeTask(models, parent, nextLink));
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

        private readonly CultureInfo culture = new CultureInfo("en-US");
        private readonly Models models;

        public BurningSeries(Models models)
        {
            this.models = models;
        }

        public string Name => "BurningSeries";

        public bool IsFileHoster => false;

        public bool Supports(string website)
        {
            return website.Contains("bs.to/");
        }

        /// <summary>
        /// verifies that the link contains the "series" specifier (and removes the hoster)
        /// with hoster: ...bs.to/serie/series-title/season/number-episode-title/language/hoster
        /// without hoster: ...bs.to/serie/series-title/season/number-episode-title/language
        /// </summary>
        /// <param name="website"></param>
        /// <param name="parts"></param>
        /// <returns>parts without hoster</returns>
        internal static string[] VerifyLinkParts(string website, string[] parts)
        {
            if (parts.Length < 5)
                throw new Exception(website + " link too short");

            if (parts[parts.Length - 5].ToLower() != "serie")
            {
                // maybe a hoster was selected?
                // ...bs.to/serie/series-title/season/number-episode-title/language/hoster
                var newParts = new string[parts.Length - 1];
                for (var i = 0; i < newParts.Length; ++i)
                    newParts[i] = parts[i];

                parts = newParts;

                if (parts.Length < 5)
                    throw new Exception(website + " link too short");

                if (parts[parts.Length - 5].ToLower() != "serie")
                    throw new Exception(website + " requires link to an episode (and not the series info)");
            }

            return parts;
        }

        public EpisodeInfo GetInfo(string website)
        {
            // ...bs.to/serie/series-title/season/number-episode-title/language
            var parts = website.Split('/');
            parts = VerifyLinkParts(website, parts);

            var series = parts[parts.Length - 4].Replace('-', ' ') + " S" + parts[parts.Length - 3];
            // extract number and series title
            var epParts = parts[parts.Length - 2].Split('-');

            if (!Int32.TryParse(epParts[0], NumberStyles.Integer, culture, out var episodeNum))
            {
                episodeNum = 0;
            }

            var episodeTitle = StringUtil.Reduce(epParts, " ", 1, epParts.Length);

            return new EpisodeInfo
            {
                SeriesTitle = series,
                EpisodeTitle = episodeTitle,
                EpisodeNumber = episodeNum
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            return new RevealHosterTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            return new NextEpisodeTask(models, parent, website);
        }

        public string FindCompatibleLink(string websiteSource)
        {
            return null;
        }
    }
}
