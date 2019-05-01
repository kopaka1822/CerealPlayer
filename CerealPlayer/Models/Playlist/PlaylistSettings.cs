using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Hoster;
using CerealPlayer.Models.Settings;

namespace CerealPlayer.Models.Playlist
{
    public class PlaylistSettings : INotifyPropertyChanged
    {
        private readonly Models models;

        public PlaylistSettings(Models models, [CanBeNull] SaveData data)
        {
            this.models = models;
            if (data != null)
            {
                CustomHosterPreferences = data.CustomHosterPreferences;
                UseCustomHosterPreferences = data.UseCustomHosterPreferences;
            }
            RefreshHosterPreferences();

            models.Web.VideoHoster.PropertyChanged += VideoHosterOnPropertyChanged;
        }

        private void VideoHosterOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(VideoHosterModel.GlobalPreferences):
                    if (!UseCustomHosterPreferences)
                    {
                        RefreshHosterPreferences();
                        OnPropertyChanged(nameof(HosterPreferences));
                    }
                    break;
            }
        }

        private bool useCustomHosterPreferences = false;
        public bool UseCustomHosterPreferences
        {
            get => useCustomHosterPreferences;
            set
            {
                if (value == useCustomHosterPreferences) return;
                useCustomHosterPreferences = value;
                RefreshHosterPreferences();
                OnPropertyChanged(nameof(UseCustomHosterPreferences));
                OnPropertyChanged(nameof(HosterPreferences));
            }
        }

        private HosterSettingsModel[] customHosterPreferences = null;
        [CanBeNull]
        public HosterSettingsModel[] CustomHosterPreferences
        {
            get => customHosterPreferences;
            set
            {
                if (ReferenceEquals(customHosterPreferences, value)) return;
                customHosterPreferences = value;
                if (UseCustomHosterPreferences)
                {
                    RefreshHosterPreferences();
                    OnPropertyChanged(nameof(HosterPreferences));
                }
                OnPropertyChanged(nameof(CustomHosterPreferences));
            }
        }

        private HosterPreferences hosterPreferences;
        public HosterPreferences HosterPreferences => hosterPreferences;

        private void RefreshHosterPreferences()
        {
            if (UseCustomHosterPreferences)
            {
                hosterPreferences = models.Web.VideoHoster.ToHosterPreferences(customHosterPreferences);
            }
            else
            {
                hosterPreferences = models.Web.VideoHoster.GlobalPreferences;
            }
        }

        public class SaveData
        {
            public bool UseCustomHosterPreferences { get; set; } = false;
            public HosterSettingsModel[] CustomHosterPreferences { get; set; } = null;
        }

        public SaveData GetSaveData()
        {
            return new SaveData
            {
                UseCustomHosterPreferences = UseCustomHosterPreferences,
                CustomHosterPreferences = CustomHosterPreferences
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
