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
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Commands.Playlist.NonLoaded;
using CerealPlayer.Views;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistCreationViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public PlaylistCreationViewModel(Models.Models models)
        {
            this.models = models;
            CancelCommand = new SetDialogResultCommand(models, false);
            CreatePlaylistCommand = new CreatePlaylistCommand(models, this);
        }

        public ICommand CancelCommand { get; }
        public ICommand CreatePlaylistCommand { get; }

        private string address = "";
        public string Address
        {
            get => address;
            set
            {
                if (value == null || address == value) return;
                address = value;
                OnPropertyChanged(nameof(Address));
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
