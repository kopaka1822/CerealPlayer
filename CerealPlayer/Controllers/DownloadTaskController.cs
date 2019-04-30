using System;
using System.Collections.Specialized;
using System.ComponentModel;
using CerealPlayer.Models;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Settings;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Controllers
{
    public class DownloadTaskController : TaskControllerBase
    {
        private int prevMaxAdvanceDownloads;
        private int prevMaxDownloads;

        public DownloadTaskController(Models.Models models) : base(models)
        {
            prevMaxDownloads = models.Settings.MaxDownloads;
            prevMaxAdvanceDownloads = models.Settings.MaxAdvanceDownloads;
            models.Settings.PropertyChanged += SettingsOnPropertyChanged;
            models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
            models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(PlaylistsModel.ActivePlaylist)) return;
            if (models.Playlists.ActivePlaylist == null) return;
            // download restruction of active playlist lifted?
            if (CanExecuteTask(models.Playlists.ActivePlaylist))
                StartNewTasks();
        }

        private void PlaylistOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null) return;
            foreach (var item in args.NewItems)
            {
                var playlist = (PlaylistModel) item;
                playlist.PropertyChanged += PlaylistOnPropertyChanged;
            }
        }

        private void PlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideo):
                    // max advance download restriction might have changed
                    StartNewTasks();
                    break;
            }
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(SettingsModel.MaxDownloads):
                    if (models.Settings.MaxDownloads > prevMaxDownloads)
                    {
                        StartNewTasks();
                    }

                    prevMaxDownloads = models.Settings.MaxDownloads;
                    break;
                case nameof(SettingsModel.MaxAdvanceDownloads):
                    if (models.Settings.MaxAdvanceDownloads > prevMaxAdvanceDownloads)
                    {
                        StartNewTasks();
                    }

                    prevMaxAdvanceDownloads = models.Settings.MaxDownloads;
                    break;
            }
        }

        protected override TaskModel GetTask(PlaylistModel playlist)
        {
            return playlist.DownloadPlaylistTask;
        }

        protected override bool CanExecuteTasks()
        {
            return NumTaskRunning < models.Settings.MaxDownloads;
        }

        protected override bool CanExecuteTask(PlaylistModel playlist)
        {
            // how many episodes ahead?
            var nextDownloadIndex = 0;
            foreach (var video in playlist.Videos)
            {
                if (video.DownloadTask.Status == TaskModel.TaskStatus.ReadyToStart)
                    break;
                nextDownloadIndex++;
            }

            if (nextDownloadIndex >= playlist.Videos.Count) return false; // nothing to do


            var episodesAhead = nextDownloadIndex - playlist.PlayingVideoIndex;
            if (episodesAhead < models.Settings.MaxAdvanceDownloads) return true;
            if (episodesAhead > models.Settings.MaxAdvanceDownloads) return false;
            // equal => is the video being played?
            return ReferenceEquals(models.Playlists.ActivePlaylist, playlist) ||
                   playlist.PlayingVideoPosition > TimeSpan.Zero;
        }
    }
}