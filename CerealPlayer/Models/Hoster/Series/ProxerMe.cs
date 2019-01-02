using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class ProxerMe : IVideoHoster
    {
        // proxer hides the hoster within a javascript string
        private abstract class RevealHoster : ISubTask
        {
            protected readonly Models models;
            private readonly TaskModel parent;
            protected readonly string website;

            private class StreamInfo
            {
                public string Code { get; set; }
                public string Replace { get; set; }
            }

            protected RevealHoster(Models models, TaskModel parent, string website)
            {
                this.parent = parent;
                this.website = website;
                this.models = models;
            }

            public async void Start()
            {
                try
                {
                    parent.Description = "resolving " + website;
                    var source = await models.Web.Html.GetAsynch(website);
                    var idx = source.IndexOf("var streams =", StringComparison.OrdinalIgnoreCase);
                    if(idx < 0)
                        throw new Exception(website + " could not find var streams =");
                    
                    // extract array
                    var arrayStart = StringUtil.SkipUntil(source, idx, '[');
                    var arrayEnd = StringUtil.SkipUntil(source, arrayStart, ']');
                    var jsArray = source.Substring(arrayStart, arrayEnd - arrayStart + 1);
                    // search for streams =
                    var res = Newtonsoft.Json.JsonConvert.DeserializeObject<StreamInfo[]>(jsArray);

                    List<string> links = new List<string>();
                    foreach (var streamInfo in res)
                    {
                        try
                        {
                            // replace # with code
                            var link = streamInfo.Replace.Replace("#", streamInfo.Code);

                            // links sometimes start with //
                            if (link.StartsWith("//"))
                                link = "https:" + link;
                            links.Add(link);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    if(links.Count == 0)
                        throw new Exception(website + " could not resolve any links in stream array");

                    await OnLinksFound(links);
                }
                catch (Exception e)
                {
                    parent.SetError(e.Message);
                }
            }

            protected abstract System.Threading.Tasks.Task OnLinksFound(List<string> links);

            public void Stop()
            {
                
            }
        }

        private class DownloadTask : RevealHoster
        {
            private readonly VideoTaskModel parent;

            public DownloadTask(Models models, VideoTaskModel parent, string website) : base(models, parent, website)
            {
                this.parent = parent;
            }

            protected override async System.Threading.Tasks.Task OnLinksFound(List<string> links)
            {
                var task = await models.Web.VideoHoster.GetHosterFromWebsitesAsynch(website, links);
                parent.SetNewSubTask(task.GetDownloadTask(parent));
            }
        }

        private class NextEpisodeTask : RevealHoster
        {
            private readonly NextEpisodeTaskModel parent;
            private readonly IVideoHoster hoster;

            public NextEpisodeTask(Models models, NextEpisodeTaskModel parent, string website, IVideoHoster hoster) : base(models, parent, website)
            {
                this.parent = parent;
                this.hoster = hoster;
            }

#pragma warning disable 1998
            protected override async System.Threading.Tasks.Task OnLinksFound(List<string> links)
#pragma warning restore 1998
            {
                Debug.Assert(links.Count != 0);
                // website exists!
                parent.Playlist.AddNextEpisode(website);
                parent.SetNewSubTask(hoster.GetNextEpisodeTask(parent, website));
            }
        }

        private readonly CultureInfo culture = new CultureInfo("en-US");
        private readonly Models models;

        public ProxerMe(Models models)
        {
            this.models = models;
        }

        public string Name => "ProxerMe";
        public bool IsFileHoster => false;

        public bool Supports(string website)
        {
            return website.Contains("proxer.me");
        }

        public EpisodeInfo GetInfo(string website)
        {
            // search the title tag in website header
            var source = System.Threading.Tasks.Task.Run(async () => { return await models.Web.Html.GetAsynch(website);}).Result;
            var idx = source.IndexOf("<title>", StringComparison.OrdinalIgnoreCase);
            if(idx < 0)
                throw new Exception(website + " could not find <title> tag");

            // series title episode number - proxer
            var title = StringUtil.SubstringUntil(source, idx + "<title>".Length, '<');
            var titleParts = title.Split(' ');
            var titleEnd = titleParts.Length - 1;
            while (titleEnd > 0 && titleParts[titleEnd].ToLower() != "episode")
            {
                --titleEnd;
            }

            var seriesTitle = StringUtil.Reduce(titleParts, " ", 0, titleEnd);

            // the episode number is in the website link
            // ...watch/id/number/language
            var addressParts = website.Split('/');
            if(addressParts.Length < 4)
                throw new Exception(website + " link too short");

            if(addressParts[addressParts.Length - 4] != "watch")
                throw new Exception(website + " requires link to an episode (not series title)");

            if (!Int32.TryParse(addressParts[addressParts.Length - 2], NumberStyles.Integer, culture,
                out var episodeNum))
            {
                episodeNum = 0;
            }

            return new EpisodeInfo
            {
                EpisodeNumber = episodeNum,
                EpisodeTitle = seriesTitle + " " + episodeNum,
                SeriesTitle = seriesTitle
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            return new DownloadTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            var nextWebsite = HosterUtil.IncrementLastNumber(website);

            return new NextEpisodeTask(models, parent, nextWebsite, this);
        }

        public string FindCompatibleLink(string websiteSource)
        {
            return null;
        }
    }
}
