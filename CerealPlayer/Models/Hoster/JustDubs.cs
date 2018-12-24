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
            private readonly TaskModel parent;

            public DownloadTask(Models models, TaskModel parent, string website)
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

                    // search for iframe src="http://www.mp4upload.com
                    var subIndex = source.IndexOf("iframe src=\"http://www.mp4upload.com/embed-", StringComparison.Ordinal);
                    if (subIndex < 0)
                    {
                        // https version
                        subIndex = source.IndexOf("iframe src=\"https://www.mp4upload.com/embed-", StringComparison.Ordinal);
                    }
                    if (subIndex < 0) throw new Exception($"failed to locate \"iframe src=\"http://www.mp4upload.com/embed-\" in {website}");

                    // get address of hoster
                    var address = StringUtil.SubstringUntil(
                        source,
                        subIndex + "iframe src=\"".Length,
                        '\"');

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

        public string GetSeriesTitle(string website)
        {
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine series title");
            var episode = website.Substring(idx + 1);
            
            // cut out the number part (everything is connected by "-")
            var parts = episode.Split('-');
            if(parts.Length == 0) throw new Exception(website + " has an empty series title");

            if (!Int32.TryParse(parts.Last(), NumberStyles.Integer, culture, out var tmpNum))
                return GetEpisodeTitle(website); // has only one episode

            var lastPartIndex = parts.Length - 1;
            if (lastPartIndex > 2)
            {
                // most links have the format: title-episode-1
                if (parts[lastPartIndex - 1] == "episode")
                    lastPartIndex--;
            }

            var res = "";
            for (var i = 0; i < lastPartIndex; ++i)
                res += parts[i] + " ";

            return res.TrimEnd(' ');
        }

        public string GetEpisodeTitle(string website)
        {
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine episode title");
            return website.Substring(idx + 1).Replace('-', ' ');
        }

        public ISubTask GetDownloadTask(TaskModel parent, string website)
        {
            return new DownloadTask(models, parent, website);
        }

        public ISubTask GetNextEpisodeTask(TaskModel parent, string website)
        {
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine next episode title");
            var episode = website.Substring(idx + 1);

            // cut out the number part (everything is connected by "-")
            var parts = episode.Split('-');
            if (parts.Length == 0) throw new Exception(website + " has an empty series title");

            if (!Int32.TryParse(parts.Last(), NumberStyles.Integer, culture, out var currentNumber))
                return null; // there is no next episode

            var nextWebsite = website.Substring(0, idx);
            for (int i = 0; i < parts.Length - 1; ++i)
                nextWebsite += parts[i] + "-";
            nextWebsite += (currentNumber + 1);

            // TODO embed next website in task
            return null;
        }
    }
}
