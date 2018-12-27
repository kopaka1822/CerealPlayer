using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster
{
    public class JustDubs : IVideoHoster
    {
        class DownloadTask : ISubTask
        {
            private readonly Models models;
            private readonly string website;
            private readonly VideoTaskModel parent;

            public DownloadTask(Models models, VideoTaskModel parent, string website)
            {
                this.website = website;
                this.parent = parent;
                this.models = models;
            }

            public async void Start()
            {
                try
                {
                    parent.Description = "resolving " + website;
                    var source = await models.Web.Html.GetAsynch(website);
                    //var source = await models.Web.Html.GetAsynch(website);

                    // search for iframe src="http://www.mp4upload.com
                    var subIndex = source.IndexOf("iframe src=\"http://www.mp4upload.com/embed-", StringComparison.OrdinalIgnoreCase);
                    if (subIndex < 0)
                    {
                        // https version
                        subIndex = source.IndexOf("iframe src=\"https://www.mp4upload.com/embed-", StringComparison.OrdinalIgnoreCase);
                    }

                    string address = null;
                    if (subIndex < 0)
                    {
                        // the link is probably hidden within player.mp4cloud.net
                        var linkLenght = "iframe src=\"https://player.mp4cloud.net/mp4upload.php?id=".Length;
                        subIndex = source.IndexOf("iframe src=\"https://player.mp4cloud.net/mp4upload.php?id=", StringComparison.OrdinalIgnoreCase);
                        if (subIndex < 0) // try http version
                        {
                            subIndex = source.IndexOf("iframe src=\"http://player.mp4cloud.net/mp4upload.php?id=", StringComparison.OrdinalIgnoreCase);
                            linkLenght--;
                        }

                        if (subIndex < 0)
                            throw new Exception($"failed to locate \"iframe src=\"http://www.mp4upload.com/embed-\" or \"iframe src=\"http://player.mp4cloud.net/mp4upload.php?id=\" in {website}");
                        
                        // the embed link is given after id
                        var id = StringUtil.SubstringUntil(
                            source,
                            subIndex + linkLenght,
                            '\"');

                        address = "https://www.mp4upload.com/embed-" + id + ".html";
                    }
                    else
                    {
                        // get address of hoster
                        address = StringUtil.SubstringUntil(
                            source,
                            subIndex + "iframe src=\"".Length,
                            '\"');
                    }

                    // give work to video hoster
                    var newHoster = models.Web.VideoHoster.GetCompatibleHoster(address);
                    parent.SetNewSubTask(newHoster.GetDownloadTask(parent, address));
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

        class NextEpisodeTask : ISubTask
        {
            private readonly Models models;
            private readonly NextEpisodeTaskModel parent;
            private readonly IVideoHoster hoster;
            private readonly string website;

            public NextEpisodeTask(Models models, NextEpisodeTaskModel parent, string website, IVideoHoster hoster)
            {
                this.models = models;
                this.parent = parent;
                this.website = website;
                this.hoster = hoster;
            }

            public async void Start()
            {
                try
                {
                    // test if the website exists
                    parent.Description = "resolving " + website;
                    var existing = await models.Web.Html.IsAvailable(website);
                    if (existing)
                    {
                        // schedule next episode and start new task
                        parent.Playlist.AddNextEpisode(website);
                        parent.SetNewSubTask(hoster.GetNextEpisodeTask(parent, website));
                    }
                    else
                    {
                        // no more episodes (for now)
                        parent.SetError("");
                    }
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

        public JustDubs(Models models)
        {
            this.models = models;
        }

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

            var seriesTitle = StringUtil.Reduce(parts, " ", lastPartIndex);

            return new EpisodeInfo
            {
                SeriesTitle = seriesTitle,
                EpisodeTitle = episodeTitle,
                EpisodeNumber = episodeNum
            };
        }

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            return new DownloadTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine next episode title");
            var episode = website.Substring(idx + 1);

            // cut out the number part (everything is connected by "-")
            var parts = episode.Split('-');
            if (parts.Length == 0) throw new Exception(website + " has an empty series title");

            if (!Int32.TryParse(parts.Last(), NumberStyles.Integer, culture, out var currentNumber))
                return null; // there is no next episode

            var nextWebsite = website.Substring(0, idx + 1);
            for (int i = 0; i < parts.Length - 1; ++i)
                nextWebsite += parts[i] + "-";
            nextWebsite += (currentNumber + 1);

            return new NextEpisodeTask(models, parent, nextWebsite, this);
        }
    }
}
