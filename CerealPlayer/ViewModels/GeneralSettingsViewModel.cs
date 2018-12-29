using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands;
using CerealPlayer.Models;

namespace CerealPlayer.ViewModels
{
    public class GeneralSettingsViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public GeneralSettingsViewModel(Models.Models models)
        {
            this.models = models;
            var settings = models.Settings;
            maxDownloads = settings.MaxDownloads;
            maxAdvanceDownloads = settings.MaxAdvanceDownloads;
            deleteAfterWatching = settings.DeleteAfterWatching;
            downloadSpeed = settings.DownloadSpeed;
            hidePlaybarTime = settings.HidePlaybarTime;
            maxChromiumInstances = settings.MaxChromiumInstances;

            CancelCommand = new SetDialogResultCommand(models, false);
            SaveCommand = new SetDialogResultCommand(models, true);
        }

        private int maxDownloads;
        private int maxAdvanceDownloads;
        private bool deleteAfterWatching;
        private int downloadSpeed;
        private int hidePlaybarTime;
        private int maxChromiumInstances;

        public int MaxDownloads
        {
            get => maxDownloads;
            set
            {
                if(value == maxDownloads) return;
                maxDownloads = Math.Max(value, 1);
                OnPropertyChanged(nameof(MaxDownloads));
            }
        }


        public int MaxAdvanceDownloads
        {
            get => maxAdvanceDownloads;
            set
            {
                if (value == maxAdvanceDownloads) return;
                maxAdvanceDownloads = Math.Max(value, 0);
                OnPropertyChanged(nameof(MaxAdvanceDownloads));
            }
        }

        public int DownloadSpeed
        {
            get => downloadSpeed;
            set
            {
                if (value == downloadSpeed) return;
                downloadSpeed = Math.Max(value, 0);
                OnPropertyChanged(nameof(DownloadSpeed));
            }
        }

        public int MaxChromium
        {
            get => maxChromiumInstances;
            set
            {
                if (value == maxChromiumInstances) return;
                maxChromiumInstances = Math.Max(value, 1);
                OnPropertyChanged(nameof(MaxChromium));
            }
        }

        public bool DeleteAfterWatching
        {
            get => deleteAfterWatching;
            set
            {
                deleteAfterWatching = value;
                OnPropertyChanged(nameof(DeleteAfterWatching));
            }
        }

        public int HidePlaybarTime
        {
            get => hidePlaybarTime;
            set
            {
                if (value == hidePlaybarTime) return;
                hidePlaybarTime = Math.Max(value, 0);
                OnPropertyChanged(nameof(HidePlaybarTime));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
