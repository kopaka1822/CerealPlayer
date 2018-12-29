using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Properties;

namespace CerealPlayer.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        /// <summary>
        /// maximum number of concurrent downloads
        /// </summary>
        public int MaxDownloads
        {
            get => Settings.Default.MaxDownloads;
            set
            {
                Debug.Assert(value > 0);
                if(Settings.Default.MaxDownloads == value) return;
                Settings.Default.MaxDownloads = value;
                OnPropertyChanged(nameof(MaxDownloads));
            }
        }

        /// <summary>
        /// maximum number of episodes that should be downloaded in advance.
        /// 0 = only download when watching
        /// 1 = download 1 episode in advance
        /// </summary>
        public int MaxAdvanceDownloads
        {
            get => Settings.Default.MaxAdvanceDownloads;
            set
            {
                Debug.Assert(value >= 0);
                if(Settings.Default.MaxAdvanceDownloads == value) return;
                Settings.Default.MaxAdvanceDownloads = value;
                OnPropertyChanged(nameof(MaxAdvanceDownloads));
            }
        }

        /// <summary>
        /// indicates if the episode should be deleted after watching
        /// </summary>
        public bool DeleteAfterWatching
        {
            get => Settings.Default.DeleteAfterWatch;
            set
            {
                if(Settings.Default.DeleteAfterWatch == value) return;
                Settings.Default.DeleteAfterWatch = value;
                OnPropertyChanged(nameof(DeleteAfterWatching));
            }
        }

        // TODO see: https://www.codeproject.com/Articles/18243/Bandwidth-throttling
        /// <summary>
        /// maximum download speed of all downloads
        /// </summary>
        public int DownloadSpeed
        {
            get => Settings.Default.DownloadSpeed;
            set
            {
                Debug.Assert(value >= 0);
                if(Settings.Default.DownloadSpeed == value) return;
                Settings.Default.DownloadSpeed = value;
                OnPropertyChanged(nameof(DownloadSpeed));
            }
        }

        public int HidePlaybarTime
        {
            get => Settings.Default.HidePlaybarTime;
            set
            {
                Debug.Assert(value >= 1);
                if(Settings.Default.HidePlaybarTime == value) return;
                Settings.Default.HidePlaybarTime = value;
                OnPropertyChanged(nameof(HidePlaybarTime));
            }
        }

        public int MaxChromiumInstances
        {
            get => Settings.Default.MaxChromiumInstances;
            set
            {
                Debug.Assert(value >= 1);
                if(Settings.Default.MaxChromiumInstances == value) return;
                Settings.Default.MaxChromiumInstances = value;
                OnPropertyChanged(nameof(MaxChromiumInstances));
            }
        }

        public void Save()
        {
            Settings.Default.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
