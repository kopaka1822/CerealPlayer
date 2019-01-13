using System.ComponentModel;
using System.Runtime.CompilerServices;
using CerealPlayer.Annotations;

namespace CerealPlayer.Models
{
    public class DisplayModel : INotifyPropertyChanged
    {
        private bool fullscreen = false;

        private bool showPlaylist = true;

        public bool Fullscreen
        {
            get => fullscreen;
            set
            {
                if (value == fullscreen) return;
                fullscreen = value;
                OnPropertyChanged(nameof(Fullscreen));
            }
        }

        public bool ShowPlaylist
        {
            get => showPlaylist;
            set
            {
                if (value == showPlaylist) return;
                showPlaylist = value;
                OnPropertyChanged(nameof(ShowPlaylist));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}