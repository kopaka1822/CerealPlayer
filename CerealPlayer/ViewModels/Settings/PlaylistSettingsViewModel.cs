using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Playlist;
using CerealPlayer.ViewModels.General;

namespace CerealPlayer.ViewModels.Settings
{
    public class PlaylistSettingsViewModel : INotifyPropertyChanged
    {
        public PlaylistSettingsViewModel(Models.Models models, PlaylistModel playlist)
        {
            useCustomHoster = playlist.Settings.UseCustomHosterPreferences;
            SaveCancel = new SaveCancelViewModel(models);
            HosterList = new HosterListViewModel(playlist.Settings.CustomHosterPreferences);
        }

        private bool useCustomHoster;
        public bool UseCustomHoster
        {
            get => useCustomHoster;
            set
            {
                if (useCustomHoster == value) return;
                useCustomHoster = value;
                OnPropertyChanged(nameof(UseCustomHoster));
            }
        }

        public SaveCancelViewModel SaveCancel { get; }

        public HosterListViewModel HosterList { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
