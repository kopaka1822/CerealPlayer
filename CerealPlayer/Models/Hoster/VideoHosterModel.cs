using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Hoster.Series;
using CerealPlayer.Models.Hoster.Stream;
using CerealPlayer.Models.Settings;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster
{
    public class VideoHosterModel
    {
        private struct HosterInfo
        {
            public IVideoHoster Hoster { get; set; }
            public bool UseHoster { get; set; }
        }

        private readonly Dictionary<string, IVideoHoster> fileHoster = new Dictionary<string, IVideoHoster>();
        private readonly List<HosterInfo> hoster = new List<HosterInfo>();
        private readonly Dictionary<string, IVideoHoster> seriesHoster = new Dictionary<string, IVideoHoster>();
        private readonly SettingsModel settings;


        public VideoHosterModel(Models models)
        {
            settings = models.Settings;

            RegisterHoster(new Openload(models));
            RegisterHoster(new Oload(models));
            RegisterHoster(new StreamAnGo(models));
            //RegisterHoster(new Streamcloud(models));
            RegisterHoster(new Mp4Upload(models));
            RegisterHoster(new RapidVideo(models));

            RegisterHoster(new JustDubs(models));
            RegisterHoster(new GoGoAnimes(models));
            RegisterHoster(new MasterAnime(models));
            RegisterHoster(new ProxerMe(models));
            RegisterHoster(new RoosterTeeth(models));
            RegisterHoster(new WatchAnime(models));
            //RegisterHoster(new BurningSeries(models));

            LoadHosterFromSettings();
            SaveHoster();

            settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(SettingsModel.PreferredHoster):
                    LoadHosterFromSettings();
                    break;
            }
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
                if(!videoHoster.UseHoster) continue;
                foreach (var pair in hosterPairs)
                {
                    if (ReferenceEquals(pair.Hoster, videoHoster))
                        return pair;
                }
            }

            throw new Exception("GetPreferredHoster - no matching hoster");
        }

        /// <summary>
        ///     adds hoster to the respective dictionary
        /// </summary>
        /// <param name="h"></param>
        private void RegisterHoster([NotNull] IVideoHoster h)
        {
            if (h.IsFileHoster)
            {
                Debug.Assert(!fileHoster.ContainsKey(h.Name));
                fileHoster.Add(h.Name, h);
            }
            else
            {
                Debug.Assert(!seriesHoster.ContainsKey(h.Name));
                seriesHoster.Add(h.Name, h);
            }
        }

        private void LoadHosterFromSettings()
        {
            hoster.Clear();
            // add all hoster according to settings list
            var preferredHoster = settings.PreferredHoster;
            var usedHoster = new HashSet<string>();
            foreach (var h in preferredHoster)
            {
                if (!fileHoster.TryGetValue(h.Name, out var res)) continue;

                hoster.Add(new HosterInfo
                {
                    Hoster = res,
                    UseHoster = h.UseHoster,
                });
                usedHoster.Add(h.Name);
            }

            if (usedHoster.Count != fileHoster.Count)
            {
                // add all hoster that were not listed in settings
                foreach (var videoHoster in fileHoster)
                {
                    if (!usedHoster.Contains(videoHoster.Key))
                    {
                        hoster.Add(new HosterInfo
                        {
                            Hoster = videoHoster.Value,
                            UseHoster = true,
                        });
                    }
                }
            }

            // add the remaining series hoster
            foreach (var videoHoster in seriesHoster)
            {
                hoster.Add(new HosterInfo
                {
                    Hoster = videoHoster.Value,
                    UseHoster = true,
                });
            }
        }

        private void SaveHoster()
        {
            var names = new List<HosterSettingsModel>();
            foreach (var videoHoster in GetFileHoster())
            {
                names.Add(new HosterSettingsModel
                {
                    Name = videoHoster.Hoster.Name,
                    UseHoster = videoHoster.UseHoster
                });
            }

            settings.PreferredHoster = names.ToArray();
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

        /// <summary>
        ///     returns all video hosters with IsFileHoster = true
        ///     sorted with their current priority
        /// </summary>
        /// <returns></returns>
        private List<HosterInfo> GetFileHoster()
        {
            return hoster.Where(videoHoster => videoHoster.Hoster.IsFileHoster).ToList();
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
    }
}