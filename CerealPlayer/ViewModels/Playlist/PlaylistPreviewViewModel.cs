using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistPreviewViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public PlaylistPreviewViewModel(Models.Models models, PlaylistPreviewView view)
        {
            this.models = models;

            CancelCommand = new SetDialogResultCommand(view, false);
            NewPlaylistCommand = new NewPlaylistCommand(models);

            foreach (var playlistName in GetPlaylistNames())
            {
                
            }   
        }

        private List<string> GetPlaylistNames()
        {
            var allDirs = Directory.GetDirectories(models.App.PlaylistDirectory);
            List<string> playlists = new List<string>();
            foreach (var dir in allDirs)
            {
                // TODO add check if this is a playlist
                playlists.Add(dir);
            }

            return playlists;
        }

        public ICommand NewPlaylistCommand { get; }

        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
