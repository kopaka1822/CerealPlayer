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
        private string videoName = "";

        public string VideoName
        {
            get => videoName;
            set
            {
                if(value == videoName) return;
                videoName = value;
                OnPropertyChanged(nameof(VideoName));
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
