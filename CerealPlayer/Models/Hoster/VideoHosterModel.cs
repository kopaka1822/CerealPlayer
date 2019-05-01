using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class VideoHosterModel : INotifyPropertyChanged
    {
        public HosterPreferences GlobalPreferences { get; set; }

        private readonly Dictionary<string, IVideoHoster> fileHoster = new Dictionary<string, IVideoHoster>();
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

        public HosterPreferences ToHosterPreferences([CanBeNull] HosterSettingsModel[] preferred)
        {
            // if a new playlist is initialized => initialize it with the global preference
            if (preferred == null && GlobalPreferences != null)
                return GlobalPreferences;

            var res = new List<HosterPreferences.HosterInfo>(fileHoster.Count + seriesHoster.Count);
            var usedHoster = new HashSet<string>();
            // add all hosters that are in settings
            if (preferred != null)
                foreach (var prefHost in preferred)
                {
                    if (!fileHoster.TryGetValue(prefHost.Name, out var videoHoster)) continue;

                    res.Add(new HosterPreferences.HosterInfo
                    {
                        Hoster = videoHoster,
                        UseHoster = prefHost.UseHoster,
                    });
                    usedHoster.Add(prefHost.Name);
                }

            // test if all hoster were used
            if (usedHoster.Count != fileHoster.Count)
            {
                // add all hoster that were not listed in settings
                foreach (var videoHoster in fileHoster)
                {
                    if (!usedHoster.Contains(videoHoster.Key))
                    {
                        res.Add(new HosterPreferences.HosterInfo
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
                res.Add(new HosterPreferences.HosterInfo
                {
                    Hoster = videoHoster.Value,
                    UseHoster = true,
                });
            }

            return new HosterPreferences(res);
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
            GlobalPreferences = ToHosterPreferences(settings.PreferredHoster);
            OnPropertyChanged(nameof(GlobalPreferences));
        }

        private void SaveHoster()
        {
            settings.PreferredHoster = GlobalPreferences.ToSettingsModels();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}