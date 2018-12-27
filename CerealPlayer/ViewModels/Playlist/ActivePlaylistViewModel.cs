﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Commands.Playlist.Video;
using CerealPlayer.Controllers;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Views;

namespace CerealPlayer.ViewModels.Playlist
{
    public class ActivePlaylistViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private PlaylistModel activePlaylist = null;

        public ActivePlaylistViewModel(Models.Models models)
        {
            this.models = models;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
            
            if(models.Playlists.ActivePlaylist != null)
                HandleNewPlaylist();
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    HandleNewPlaylist();
                    break;
            }
        }

        private void HandleNewPlaylist()
        {
            Reset();
            if (models.Playlists.ActivePlaylist == null) return;

            activePlaylist = models.Playlists.ActivePlaylist;
            // add existing videos
            foreach (var playlistVideo in activePlaylist.Videos)
            {
                var view = new PlaylistItemView
                {
                    DataContext = new PlaylistItemViewModel(models, playlistVideo, 
                        new RetryVideoDownloadCommand(activePlaylist))
                };
                Videos.Add(view);
            }

            // add future videos
            models.Playlists.ActivePlaylist.Videos.CollectionChanged += VideosOnCollectionChanged;
        }

        private void VideosOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            Debug.Assert(args.Action == NotifyCollectionChangedAction.Add);
            Debug.Assert(args.NewStartingIndex == Videos.Count);

            // add to the end of the list
            Videos.Add(new PlaylistItemView
            {
                DataContext = new PlaylistItemViewModel(models, activePlaylist.Videos.Last(),
                    new RetryVideoDownloadCommand(activePlaylist))
            });
        }

        private void Reset()
        {
            // remove all event subscriptions
            if (activePlaylist != null)
            {
                activePlaylist.Videos.CollectionChanged -= VideosOnCollectionChanged;
            }
            Videos.Clear();
            SelectedVideo = null;
            activePlaylist = null;
        }

        public ObservableCollection<PlaylistItemView> Videos { get; } = new ObservableCollection<PlaylistItemView>();

        private PlaylistItemView selectedVideo = null;

        public PlaylistItemView SelectedVideo
        {
            get => selectedVideo;
            set
            {
                if (ReferenceEquals(value, selectedVideo)) return;
                selectedVideo = value;
                OnPropertyChanged(nameof(SelectedVideo));
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
