using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using CerealPlayer.Models.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistsPreviewViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public PlaylistsPreviewViewModel(Models.Models models, PlaylistsPreviewView view)
        {
            this.models = models;

            CancelCommand = new SetDialogResultCommand(view, false);
            NewPlaylistCommand = new NewPlaylistCommand(models);

            RefreshPlaylist();

            this.models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
        }

        private void PlaylistOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RefreshPlaylist();
        }

        private void RefreshPlaylist()
        {      
            var allDirs = Directory.GetDirectories(models.App.PlaylistDirectory);
            PlaylistItems.Clear();

            foreach (var dir in allDirs)
            {
                var dirName = Path.GetFileName(dir);
                // is already open?
                foreach (var openPlaylist in models.Playlists.List)
                {
                    if (openPlaylist.Name == dirName)
                    {
                        // this playlist is already open
                        var v = new PlaylistTaskView
                        {
                            DataContext = new LoadedPlaylistTaskViewModel(models, openPlaylist)
                        };
                        PlaylistItems.Add(v);

                        goto dirloop;
                    }
                }
                
                // test if this is a valid folder
                var saveFileLocation = PlaylistModel.GetSettingsLocation(dir);
                if(!File.Exists(saveFileLocation)) continue;

                // add file that is not open
                var view = new PlaylistTaskView
                {
                    DataContext = new NonLoadedPlaylistTaskModel(models, dir)
                };
                PlaylistItems.Add(view);

                dirloop:;
            }
        }

        public ICommand NewPlaylistCommand { get; }

        public ICommand CancelCommand { get; }

        public ObservableCollection<object> PlaylistItems { get; } = new ObservableCollection<object>();

        private object selectedPlaylist = null;

        public object SelectedPlaylist
        {
            get => selectedPlaylist;
            set
            {
                if (ReferenceEquals(value, selectedPlaylist)) return;
                selectedPlaylist = value;
                OnPropertyChanged(nameof(SelectedPlaylist));
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
