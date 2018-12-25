using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.ViewModels.Player
{
    public class PlayerViewModel
    {
        private readonly Models.Models models;
        private readonly MediaElement player;
        private PlaylistModel activePlaylist = null;
        private VideoModel activeVideo = null;

        public PlayerViewModel(Models.Models models)
        {
            this.models = models;
            this.player = models.App.Window.Player;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
            player.MediaEnded += PlayerOnMediaEnded;
            player.MediaFailed += PlayerOnMediaFailed;
            player.MediaOpened += PlayerOnMediaOpened;
            player.BufferingStarted += PlayerOnBufferingStarted;
            player.LoadedBehavior = MediaState.Manual;
            player.UnloadedBehavior = MediaState.Manual;
        }

        private void PlayerOnBufferingStarted(object sender, RoutedEventArgs routedEventArgs)
        {
            int a = 3;
        }

        private void PlayerOnMediaOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            player.Play();
        }

        private void PlayerOnMediaFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            // cannot find file?
            int a = 3;
        }

        private void PlayerOnMediaEnded(object sender, RoutedEventArgs routedEventArgs)
        {
            // play next video
            if (activePlaylist != null)
            {
                activePlaylist.PlayingVideoIndex = activePlaylist.PlayingVideoIndex + 1;
            }
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    StartNewPlaylist();
                    break;
            }
        }

        private void StartNewPlaylist()
        {
            // unsibsribe old events
            if (activePlaylist != null)
            {
                activePlaylist.PropertyChanged -= ActivePlaylistOnPropertyChanged;
            }

            player.Stop();
            activePlaylist = models.Playlists.ActivePlaylist;
            if (activePlaylist == null) return;

            activePlaylist.PropertyChanged += ActivePlaylistOnPropertyChanged;
            // where did we left of?
            var idx = activePlaylist.PlayingVideoIndex;
            PlayVideo(activePlaylist.Videos[idx]);
        }

        private void ActivePlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideoIndex):
                    PlayVideo(activePlaylist.Videos[activePlaylist.PlayingVideoIndex]);
                    break;
            }
        }

        public void PlayVideo(VideoModel video)
        {
            // unsubsribe from old event
            if (activeVideo != null) activeVideo.DownloadTask.PropertyChanged -= DownloadTaskOnPropertyChanged;

            activeVideo = video;
            player.Source = null;
            if (activeVideo == null) return;

            // subscribe to new event
            video.DownloadTask.PropertyChanged += DownloadTaskOnPropertyChanged;
            switch (video.DownloadTask.Status)
            {
                case TaskModel.TaskStatus.Failed:
                case TaskModel.TaskStatus.NotStarted:
                    // wait for start
                    break;
                case TaskModel.TaskStatus.Running:
                    // start if video has started downloading
                    /*if (video.DownloadTask.Percentage > 0)
                    {
                        player.Source = new Uri(video.FileLocation);
                    }*/ // for now wait until it is finished downloading
                    break;
                case TaskModel.TaskStatus.Finished:
                    // start immediately
                    player.Source = new Uri(video.FileLocation);
                    break;
            }
        }

        private void DownloadTaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // download task of the active video
            switch (args.PropertyName)
            {
                case nameof(TaskModel.Status):
                    switch (activeVideo.DownloadTask.Status)
                    {
                        case TaskModel.TaskStatus.Finished:
                            if (player.Source == null)
                            {
                                player.Source = new Uri(activeVideo.FileLocation);
                            }
                            break;
                        case TaskModel.TaskStatus.Failed:
                            MessageBox.Show("download failed");
                            // oh no :(
                            break;
                    }
                    break;
                case nameof(TaskModel.Percentage):
                    /*if (player.Source == null && activeVideo.DownloadTask.Percentage > 0)
                    {
                        // set source
                        player.Source = new Uri(activeVideo.FileLocation);
                    }*/
                    break;
            }
        }
    }
}

