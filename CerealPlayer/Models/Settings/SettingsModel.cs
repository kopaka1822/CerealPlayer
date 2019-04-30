using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using CerealPlayer.Annotations;
using Newtonsoft.Json;

namespace CerealPlayer.Models.Settings
{
    public class SettingsModel : INotifyPropertyChanged
    {
        private int maxDownloads = 3;
        /// <summary>
        ///     maximum number of concurrent downloads
        /// </summary>
        public int MaxDownloads
        {
            get => maxDownloads;
            set
            {
                Debug.Assert(value > 0);
                if (maxDownloads == value) return;
                maxDownloads = value;
                OnPropertyChanged(nameof(MaxDownloads));
            }
        }

        private int maxAdvanceDownloads = 24;
        /// <summary>
        ///     maximum number of episodes that should be downloaded in advance.
        ///     0 = only download when watching
        ///     1 = download 1 episode in advance
        /// </summary>
        public int MaxAdvanceDownloads
        {
            get => maxAdvanceDownloads;
            set
            {
                Debug.Assert(value >= 0);
                if (maxAdvanceDownloads == value) return;
                maxAdvanceDownloads = value;
                OnPropertyChanged(nameof(MaxAdvanceDownloads));
            }
        }

        private bool deleteAfterWatching = false;
        /// <summary>
        ///     indicates if the episode should be deleted after watching
        /// </summary>
        public bool DeleteAfterWatching
        {
            get => deleteAfterWatching;
            set
            {
                if (deleteAfterWatching == value) return;
                deleteAfterWatching = value;
                OnPropertyChanged(nameof(DeleteAfterWatching));
            }
        }

        private int downloadSpeed = 0;
        // TODO see: https://www.codeproject.com/Articles/18243/Bandwidth-throttling
        /// <summary>
        ///     maximum download speed of all downloads
        /// </summary>
        public int DownloadSpeed
        {
            get => downloadSpeed;
            set
            {
                Debug.Assert(value >= 0);
                if (downloadSpeed == value) return;
                downloadSpeed = value;
                OnPropertyChanged(nameof(DownloadSpeed));
            }
        }

        private int hidePlaybarTime = 5;
        public int HidePlaybarTime
        {
            get => hidePlaybarTime;
            set
            {
                Debug.Assert(value >= 1);
                if (hidePlaybarTime == value) return;
                hidePlaybarTime = value;
                OnPropertyChanged(nameof(HidePlaybarTime));
            }
        }

        private int maxChromiumInstances = 6;
        public int MaxChromiumInstances
        {
            get => maxChromiumInstances;
            set
            {
                Debug.Assert(value >= 1);
                if (maxChromiumInstances == value) return;
                maxChromiumInstances = value;
                OnPropertyChanged(nameof(MaxChromiumInstances));
            }
        }

        private HosterSettingsModel[] preferredHoster = new HosterSettingsModel[0];
        /// <summary>
        ///     list with the names of the preferred file hoster
        /// </summary>
        public HosterSettingsModel[] PreferredHoster
        {
            get => preferredHoster;
            set
            {
                Debug.Assert(value != null);
                preferredHoster = value;
                OnPropertyChanged(nameof(PreferredHoster));
            }
        }

        private int rewindOnPlaylistChangeTime = 0;
        public int RewindOnPlaylistChangeTime
        {
            get => rewindOnPlaylistChangeTime;
            set
            {
                Debug.Assert(value >= 0);
                if (value == rewindOnPlaylistChangeTime) return;
                rewindOnPlaylistChangeTime = value;
                OnPropertyChanged(nameof(RewindOnPlaylistChangeTime));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText("settings.json", json);
        }

        public static SettingsModel Load()
        {
            try
            {
                var json = File.ReadAllText("settings.json");
                var res = JsonConvert.DeserializeObject<SettingsModel>(json);
                return res;
            }
            catch (Exception)
            {
                // keep default settings
                return new SettingsModel();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}