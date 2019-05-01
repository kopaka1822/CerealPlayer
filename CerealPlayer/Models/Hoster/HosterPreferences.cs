using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Settings;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster
{
    public class HosterPreferences
    {
        public class HosterInfo
        {
            public IVideoHoster Hoster { get; set; }
            public bool UseHoster { get; set; }
        }

        public class WebsiteHosterPair
        {
            public string Website { set; get; }
            public IVideoHoster Hoster { set; get; }

            public ISubTask GetDownloadTask(VideoTaskModel parent)
            {
                return Hoster.GetDownloadTask(parent, Website);
            }

            public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent)
            {
                return Hoster.GetNextEpisodeTask(parent, Website);
            }
        }

        private readonly List<HosterInfo> hoster;

        public HosterPreferences(List<HosterInfo> hoster)
        {
            this.hoster = hoster;
        }

        /// <summary>
        /// converts all file hosters into a HosterSettingsModel array
        /// </summary>
        /// <returns></returns>
        public HosterSettingsModel[] ToSettingsModels()
        {
            var names = new List<HosterSettingsModel>();
            foreach (var videoHoster in hoster)
            {
                if(!videoHoster.Hoster.IsFileHoster) continue;
                names.Add(new HosterSettingsModel
                {
                    Name = videoHoster.Hoster.Name,
                    UseHoster = videoHoster.UseHoster
                });
            }

            return names.ToArray();
        }

        /// <summary>
        ///     returns the preferred hoster according to the priority list
        /// </summary>
        /// <param name="hosterPairs"></param>
        /// <returns></returns>
        public WebsiteHosterPair GetPreferredHoster(List<WebsiteHosterPair> hosterPairs)
        {
            Debug.Assert(hosterPairs.Count > 0);

            foreach (var videoHoster in hoster)
            {
                if (!videoHoster.UseHoster) continue;
                foreach (var pair in hosterPairs)
                {
                    if (ReferenceEquals(pair.Hoster, videoHoster.Hoster))
                        return pair;
                }
            }

            throw new Exception("GetPreferredHoster - no matching hoster");
        }

        /// <summary>
        ///     tries to find a compatible video hoster
        /// </summary>
        /// <param name="website"></param>
        /// <exception cref="Exception">thrown if no hoster was found</exception>
        /// <returns></returns>
        public IVideoHoster GetCompatibleHoster([NotNull] string website)
        {
            foreach (var videoHoster in hoster)
            {
                if (!videoHoster.UseHoster) continue;
                if (videoHoster.Hoster.Supports(website)) return videoHoster.Hoster;
            }

            throw new Exception("no compatible hoster for " + website);
        }

        /// <summary>
        ///     tries to find a compatible hoster for the given website
        /// </summary>
        /// <param name="website">parent website (for exception message)</param>
        /// <param name="websites">potential video hoster websites</param>
        /// <returns></returns>
        public async Task<WebsiteHosterPair> GetHosterFromWebsitesAsynch([NotNull] string website,
            [NotNull] List<string> websites)
        {
            var task = await System.Threading.Tasks.Task.Run(() =>
            {
                foreach (var videoHoster in hoster)
                {
                    if (!videoHoster.UseHoster) continue;
                    foreach (var site in websites)
                    {
                        if (videoHoster.Hoster.Supports(site))
                            return new WebsiteHosterPair
                            {
                                Hoster = videoHoster.Hoster,
                                Website = site
                            };
                    }
                }

                return null;
            });

            if (task == null)
                throw new Exception("no compatible hosters found on " + website + " [" +
                                    StringUtil.Reduce(websites.ToArray(), ", ") + "]");

            return task;
        }

        /// <summary>
        ///     tries to find a compatible hoster on given website
        /// </summary>
        /// <param name="website">website address</param>
        /// <param name="source">source of the website</param>
        /// <returns></returns>
        public async Task<WebsiteHosterPair> GetHosterFromSourceAsynch([NotNull] string website,
            [NotNull] string source)
        {
            var task = await System.Threading.Tasks.Task.Run(() =>
            {
                foreach (var videoHoster in hoster)
                {
                    if (!videoHoster.UseHoster) continue;
                    var link = videoHoster.Hoster.FindCompatibleLink(source);
                    if (link != null)
                        return new WebsiteHosterPair
                        {
                            Hoster = videoHoster.Hoster,
                            Website = link
                        };
                }

                return null;
            });

            if (task == null)
                throw new Exception("no compatible hosters found on " + website);

            return task;
        }
    }
}
