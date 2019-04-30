using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;
using CerealPlayer.Views;
using CerealPlayer.Views.Playlist;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistsPreviewViewModel : INotifyPropertyChanged
    {
        private readonly Dictionary<string, LoadedTaskInfo> loadedViews = new Dictionary<string, LoadedTaskInfo>();
        private readonly Models.Models models;

        private object selectedPlaylist = null;

        public PlaylistsPreviewViewModel(Models.Models models)
        {
            this.models = models;

            Debug.Assert(models.Playlists.List.Count == 0);

            RefreshPlaylist();

            this.models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
            this.models.Playlists.DirectoryRefresh += PlaylistsOnDirectoryRefresh;
        }

        public ObservableCollection<PlaylistTaskView> PlaylistItems { get; } = new ObservableCollection<PlaylistTaskView>();

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

        private void PlaylistsOnDirectoryRefresh(object sender, EventArgs eventArgs)
        {
            RefreshPlaylist();
        }

        private void SortPlaylists()
        {
            // sort by loaded first
            var tmp = PlaylistItems.OrderByDescending(o => o.DataContext is LoadedPlaylistTaskViewModel);
            // sort by is 
            tmp = tmp.ToArray().OrderByDescending(o =>
            {
                if (!(o.DataContext is LoadedPlaylistTaskViewModel)) return false;
                return ((LoadedPlaylistTaskViewModel) o.DataContext).HasEpisodesLeft;
            });

            // refresh playlist items
            PlaylistItems.Clear();
            foreach (var i in tmp)
            {
                PlaylistItems.Add(i);
            }
        }

        /// <summary>
        ///     loads all playlists from the playlist folder and updates them
        /// </summary>
        public void UpdateAllPlaylists()
        {
            var allDirs = Directory.GetDirectories(models.App.PlaylistDirectory);
            // ignore collection changed (lots of insertions)
            models.Playlists.List.CollectionChanged -= PlaylistOnCollectionChanged;
            PlaylistItems.Clear();

            foreach (var dir in allDirs)
            {
                var dirname = Path.GetFileName(dir);
                if (dirname == null) continue;
                if (loadedViews.TryGetValue(dirname, out var loadedView))
                {
                    // this was already loaded
                    PlaylistItems.Add(loadedView.View);
                    if (loadedView.Model.NextEpisodeTask.Status == TaskModel.TaskStatus.Failed)
                        loadedView.Model.NextEpisodeTask.ResetStatus();
                    if (loadedView.Model.DownloadPlaylistTask.Status == TaskModel.TaskStatus.Failed)
                        loadedView.Model.DownloadPlaylistTask.ResetStatus();
                    continue;
                }

                // test if this is a valid folder
                var saveFileLocation = PlaylistModel.GetSettingsLocation(dir);
                if (!File.Exists(saveFileLocation)) continue;

                // valid folder => load playlist
                try
                {
                    var model = PlaylistModel.LoadFromDirectory(dir, models);
                    models.Playlists.List.Add(model);

                    var view = CreateView(model);
                    PlaylistItems.Add(view);
                    
                }
                catch (Exception e)
                {
                    // ignore this one
                    Console.WriteLine(e.Message);
                }
            }

            SortPlaylists();

            // resubscribe to change events
            models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
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
                    CreateView(playlist);
                }
            }

            RefreshPlaylist();
        }

        private PlaylistTaskView CreateView(PlaylistModel playlist)
        {
            var dc = new LoadedPlaylistTaskViewModel(models, playlist);
            var view = new PlaylistTaskView
            {
                DataContext = dc
            };
            loadedViews.Add(playlist.Name, new LoadedTaskInfo
            {
                View = view,
                Model = playlist
            });
            dc.PropertyChanged += LoadedPlaylistOnPropertyChanged;
            return view;
        }

        private void LoadedPlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(LoadedPlaylistTaskViewModel.HasEpisodesLeft):
                    SortPlaylists();
                    break;
            }
        }

        private void RefreshPlaylist()
        {
            var allDirs = Directory.GetDirectories(models.App.PlaylistDirectory);
            PlaylistItems.Clear();

            foreach (var dir in allDirs)
            {
                var dirname = Path.GetFileName(dir);
                if (dirname == null) continue;
                if (loadedViews.TryGetValue(dirname, out var loadedView))
                {
                    PlaylistItems.Add(loadedView.View);
                    continue;
                }

                // test if this is a valid folder
                var saveFileLocation = PlaylistModel.GetSettingsLocation(dir);
                if (!File.Exists(saveFileLocation)) continue;

                // add file that is not open
                var view = new PlaylistTaskView
                {
                    DataContext = new NonLoadedPlaylistTaskModel(models, dir)
                };
                PlaylistItems.Add(view);
            }

            SortPlaylists();
            OnPropertyChanged(nameof(PlaylistItems));
            OnPropertyChanged(nameof(SelectedPlaylist));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private struct LoadedTaskInfo
        {
            public PlaylistTaskView View { get; set; }
            public PlaylistModel Model { get; set; }
        }
    }
}