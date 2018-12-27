using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;

namespace CerealPlayer.Models
{
    public class DisplayModel : INotifyPropertyChanged
    {
        private bool fullscreen = false;

        public bool Fullscreen
        {
            get => fullscreen;
            set
            {
                if(value == fullscreen) return;
                fullscreen = value;
                OnPropertyChanged(nameof(Fullscreen));
            }
        }

        private bool showPlaylist = true;

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
