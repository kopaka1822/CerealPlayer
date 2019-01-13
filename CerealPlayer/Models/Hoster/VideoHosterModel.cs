using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Hoster.Series;
using CerealPlayer.Models.Hoster.Stream;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster
{
    public class VideoHosterModel
    {
        private readonly Dictionary<string, IVideoHoster> fileHoster = new Dictionary<string, IVideoHoster>();
        private readonly List<IVideoHoster> hoster = new List<IVideoHoster>();
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
        private void RegisterHoster(IVideoHoster h)
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
            foreach (var hosterName in preferredHoster)
            {
                if (fileHoster.TryGetValue(hosterName, out var res))
                {
                    hoster.Add(res);
                    usedHoster.Add(hosterName);
                }
            }

            if (usedHoster.Count != fileHoster.Count)
            {
                // add all hoster that were not listed in settings
                foreach (var videoHoster in fileHoster)
                {
                    if (!usedHoster.Contains(videoHoster.Key))
                    {
                        hoster.Add(videoHoster.Value);
                    }
                }
            }

            // add the remaining series hoster
            foreach (var videoHoster in seriesHoster)
            {
                hoster.Add(videoHoster.Value);
            }
        }

        private void SaveHoster()
        {
            var names = new List<string>();
            foreach (var videoHoster in GetFileHoster())
            {
                names.Add(videoHoster.Name);
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
                if (videoHoster.Supports(website)) return videoHoster;
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
                    foreach (var site in websites)
                    {
                        if (videoHoster.Supports(site))
                            return new WebsiteHosterPair
                            {
                                Hoster = videoHoster,
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
                    var link = videoHoster.FindCompatibleLink(source);
                    if (link != null)
                        return new WebsiteHosterPair
                        {
                            Hoster = videoHoster,
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
        private List<IVideoHoster> GetFileHoster()
        {
            return hoster.Where(videoHoster => videoHoster.IsFileHoster).ToList();
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