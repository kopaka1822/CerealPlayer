using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;

namespace CerealPlayer.Models.Player
{
    public class PlayerModel : INotifyPropertyChanged
    {
        private bool isPausing = false;

        public bool IsPausing
        {
            get => isPausing;
            set
            {
                if(value == isPausing) return;
                isPausing = value;
                OnPropertyChanged(nameof(IsPausing));
            }
        }

        private double volume = 1.0;

        public double Volume
        {
            get => volume;
            set
            {
                if(Equals(value, volume)) return;
                volume = value;
                OnPropertyChanged(nameof(Volume));
            }
        }

        private bool isPlayerBarVisible = true;

        public bool IsPlayerBarVisible
        {
            get => isPlayerBarVisible;
            set
            {
                if(value == isPlayerBarVisible) return;
                isPlayerBarVisible = value;
                OnPropertyChanged(nameof(IsPlayerBarVisible));
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
