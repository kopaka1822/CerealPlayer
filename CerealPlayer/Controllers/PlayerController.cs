using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CerealPlayer.Models.Player;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Controllers
{
    public class PlayerController
    {
        private readonly Models.Models models;

        // indicates when the last mouse movement happened
        private readonly Stopwatch mouseMoveStopwatch = new Stopwatch();
        private readonly MediaElement player;
        private PlaylistModel activePlaylist = null;
        private VideoModel activeVideo = null;
        private Point lastMousePosition = new Point(0, 0);

        public PlayerController(Models.Models models)
        {
            this.models = models;
            player = models.App.Window.Player;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
            player.MediaEnded += PlayerOnMediaEnded;
            player.MediaFailed += PlayerOnMediaFailed;
            player.MediaOpened += PlayerOnMediaOpened;
            player.BufferingStarted += PlayerOnBufferingStarted;
            player.LoadedBehavior = MediaState.Manual;
            player.UnloadedBehavior = MediaState.Manual;
            player.Volume = models.Player.Volume;

            models.Player.PropertyChanged += PlayerOnPropertyChanged;

            // set up a time to refresh control bar
            var timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Start();

            // hide mouse events
            models.App.Window.MouseMove += WindowOnMouseMove;
            mouseMoveStopwatch.Start();
        }

        private bool HasMedia => player.Source != null;

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlayerModel.IsPausing):
                    if (models.Player.IsPausing)
                    {
                        if (HasMedia)
                        {
                            player.Pause();
                            activePlaylist.PlayingVideoPosition = player.Position;
                        }
                    }
                    else
                    {
                        if (HasMedia)
                        {
                            player.Play();
                            // set play position after pressing play (because play would reset it..)
                            player.Position = activePlaylist.PlayingVideoPosition;
                        }
                    }

                    break;
                case nameof(PlayerModel.Volume):
                    player.Volume = models.Player.Volume;
                    break;
            }
        }

        private void WindowOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var newPoint = mouseEventArgs.GetPosition(models.App.Window);
            if (newPoint != lastMousePosition)
                mouseMoveStopwatch.Restart();
            lastMousePosition = newPoint;
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (activePlaylist != null
                && player.Position != TimeSpan.Zero) // ignore this => media is still loading
            {
                // ignore own video position changed events
                activePlaylist.PlayingVideoPositionChanged -= OnPlayingVideoPositionChanged;
                activePlaylist.PlayingVideoPosition = player.Position;
                activePlaylist.PlayingVideoPositionChanged += OnPlayingVideoPositionChanged;
            }

            // hide play bar?
            if (models.App.Window.PlayerBar.IsMouseOver) mouseMoveStopwatch.Restart();

            if (HasMedia && !models.Player.IsPausing &&
                mouseMoveStopwatch.Elapsed > TimeSpan.FromSeconds(models.Settings.HidePlaybarTime))
            {
                models.Player.IsPlayerBarVisible = false;
            }
            else
            {
                models.Player.IsPlayerBarVisible = true;
            }
        }

        private void PlayerOnBufferingStarted(object sender, RoutedEventArgs routedEventArgs)
        {
            //int a = 3;
        }

        private void PlayerOnMediaOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            activePlaylist.PlayingVideoDuration = player.NaturalDuration.TimeSpan;
            if (!models.Player.IsPausing)
            {
                player.Play();
            }

            player.Position = activePlaylist.PlayingVideoPosition;
        }

        private void PlayerOnMediaFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            // cannot find file?
            MessageBox.Show(models.App.TopmostWindow, exceptionRoutedEventArgs.ErrorException.Message, "Playback Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
                activePlaylist.PlayingVideoPositionChanged -= OnPlayingVideoPositionChanged;
            }

            player.Stop();
            player.Source = null;
            activePlaylist = models.Playlists.ActivePlaylist;
            if (activePlaylist == null) return;

            activePlaylist.PropertyChanged += ActivePlaylistOnPropertyChanged;
            activePlaylist.PlayingVideoPositionChanged += OnPlayingVideoPositionChanged;
            // where did we left of?
            PlayVideo(activePlaylist.PlayingVideo);
        }

        private void OnPlayingVideoPositionChanged(object sender, EventArgs eventArgs)
        {
            player.Position = activePlaylist.PlayingVideoPosition;
        }

        private void ActivePlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideo):
                    PlayVideo(activePlaylist.PlayingVideo);
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
                case TaskModel.TaskStatus.ReadyToStart:
                    // wait for start
                    break;
                case TaskModel.TaskStatus.Running:
                    // start if video has started downloading
                    /*if (video.DownloadTask.Progress > 0)
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
                            MessageBox.Show(models.App.TopmostWindow, "download failed");
                            // oh no :(
                            break;
                    }

                    break;
                case nameof(TaskModel.Progress):
                    /*if (player.Source == null && activeVideo.DownloadTask.Progress > 0)
                    {
                        // set source
                        player.Source = new Uri(activeVideo.FileLocation);
                    }*/
                    break;
            }
        }
    }
}