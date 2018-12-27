using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Commands.Playlist.NonLoaded;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistsPreviewViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        private readonly Dictionary<string, PlaylistTaskView> loadedViews = new Dictionary<string, PlaylistTaskView>();

        public PlaylistsPreviewViewModel(Models.Models models)
        {
            this.models = models;

            CancelCommand = new SetDialogResultCommand(models, false);
            NewPlaylistCommand = new NewPlaylistCommand(models);

            Debug.Assert(models.Playlists.List.Count == 0);

            RefreshPlaylist();

            this.models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
            this.models.Playlists.DirectoryRefresh += PlaylistsOnDirectoryRefresh;
        }

        private void PlaylistsOnDirectoryRefresh(object sender, EventArgs eventArgs)
        {
            RefreshPlaylist();
        }

        private void PlaylistOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
            {
                foreach (var item in args.OldItems)
                {
                    var playlist = (PlaylistModel) item;
                    loadedViews.Remove(playlist.Name);
                }
            }

            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems)
                {
                    var playlist = (PlaylistModel) item;
                    var view = new PlaylistTaskView
                    {
                        DataContext = new LoadedPlaylistTaskViewModel(models, playlist)
                    };
                    loadedViews.Add(playlist.Name, view);
                }
            }
            RefreshPlaylist();
        }

        private void RefreshPlaylist()
        {      
            var allDirs = Directory.GetDirectories(models.App.PlaylistDirectory);
            PlaylistItems.Clear();

            foreach (var dir in allDirs)
            {
                var dirname = Path.GetFileName(dir);
                if(dirname == null) continue;
                if (loadedViews.TryGetValue(dirname, out var loadedView))
                {
                    PlaylistItems.Add(loadedView);
                    continue;
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
            }
            OnPropertyChanged(nameof(PlaylistItems));
            OnPropertyChanged(nameof(SelectedPlaylist));
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
