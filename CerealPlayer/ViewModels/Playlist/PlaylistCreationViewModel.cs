using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands;
using CerealPlayer.Commands.Playlist.NonLoaded;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistCreationViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        private string address = "";

        private bool play = true;

        public PlaylistCreationViewModel(Models.Models models)
        {
            this.models = models;
            CancelCommand = new SetDialogResultCommand(models, false);
            CreatePlaylistCommand = new CreatePlaylistCommand(models, this);
        }

        public ICommand CancelCommand { get; }
        public ICommand CreatePlaylistCommand { get; }

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

        public bool Play
        {
            get => play;
            set
            {
                if (value == play) return;
                play = value;
                OnPropertyChanged(nameof(Play));
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