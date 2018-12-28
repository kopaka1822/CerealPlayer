using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CerealPlayer.Annotations;
using CerealPlayer.Commands;
using CerealPlayer.Commands.Player;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.ViewModels.Player
{
    public class PlayerViewModel : INotifyPropertyChanged
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
            // default max volume
            player.Volume = 1.0;

            PlayCommand = new PlayPauseCommand(this);

            PreviousEpisodeCommand = new ChangeEpisodeCommand(models, -1);
            NextEpisodeCommand = new ChangeEpisodeCommand(models, 1);

            WindBackCommand = new WindPlayerCommand(this, TimeSpan.FromSeconds(-10));
            WindForwardCommand = new WindPlayerCommand(this, TimeSpan.FromSeconds(30));

            ToggleFullscreenCommand = new ToggleFullscreenCommand(models);
            TogglePlaylistCommand = new TogglePlaylistCommand(models);

            // set up a time to refresh control bar
            var timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Start();

            // dont update slider if user is holding slider..
            var slider = models.App.Window.PlayerBar.Slider;

            slider.PreviewMouseDown += SliderOnPreviewMouseDown;
            slider.PreviewMouseUp += SliderOnPreviewMouseUp;

            // hide mouse events
            models.App.Window.MouseMove += WindowOnMouseMove;
            mouseMoveStopwatch.Start();
        }

        // indicates when the last mouse movement happened
        private readonly Stopwatch mouseMoveStopwatch = new Stopwatch();
        private Point lastMousePosition = new Point(0, 0);

        private void WindowOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var newPoint = mouseEventArgs.GetPosition(models.App.Window);
            if(newPoint != lastMousePosition)
                mouseMoveStopwatch.Restart();
            lastMousePosition = newPoint;
        }

        private Visibility barVisibility = Visibility.Visible;

        public Visibility BarVisibility
        {
            get => barVisibility;
            set
            {
                if (value == barVisibility) return;
                barVisibility = value;
                OnPropertyChanged(nameof(BarVisibility));
            }
        }

        private void SliderOnPreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            sliderOccupied = false;
        }

        private void SliderOnPreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            sliderOccupied = true;
        }

        // indicates if the user is changing the slider
        private bool sliderOccupied = false;

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(TimeElapsed));
            OnPropertyChanged(nameof(TimeRemaining));
            if (!sliderOccupied)
                OnPropertyChanged(nameof(TimeProgress));

            // hide play bar?
            if(models.App.Window.PlayerBar.IsMouseOver) mouseMoveStopwatch.Restart();

            if (hasMedia && !isPausing && mouseMoveStopwatch.Elapsed > TimeSpan.FromSeconds(5))
            {
                BarVisibility = Visibility.Collapsed;
            }
            else
            {
                if (BarVisibility == Visibility.Collapsed)
                    while (false) { }

                BarVisibility = Visibility.Visible;
            }
        }

        private bool hasMedia => player.Source != null;

        private bool _isPausing = false;
        private bool isPausing
        {
            get => _isPausing;
            set
            {
                if (value == _isPausing) return;
                _isPausing = value;
                OnPropertyChanged(nameof(PlayVisibility));
                OnPropertyChanged(nameof(PauseVisibility));
            }
        }

        public Visibility PauseVisibility => isPausing ? Visibility.Collapsed : Visibility.Visible;

        public Visibility PlayVisibility => isPausing ? Visibility.Visible : Visibility.Collapsed;

        private string episodeTitle = "";
        public string EpisodeTitle
        {
            get => episodeTitle;
            set
            {
                if (episodeTitle == value) return;
                episodeTitle = value;
                OnPropertyChanged(nameof(EpisodeTitle));
            }
        }

        // public properties
        public double Volume
        {
            get => player.Volume;
            set
            {
                if(Equals(value, player.Volume)) return;
                player.Volume = value;
                OnPropertyChanged(nameof(Volume));
            }
        }

        private readonly string noMediaTime = "--:--:--";

        public string TimeElapsed => hasMedia ? player.Position.ToString(@"hh\:mm\:ss") : noMediaTime;

        public string TimeRemaining
        {
            get
            {
                if (!hasMedia) return noMediaTime;
                var pos = player.Position;
                var dur = player.NaturalDuration;
                var res = dur.TimeSpan.Subtract(pos);
                return res.ToString(@"hh\:mm\:ss");
            }
        }

        public double TimeProgress
        {
            get
            {
                if (!hasMedia) return 0;
                var ratio = (double)player.Position.Ticks / player.NaturalDuration.TimeSpan.Ticks;
                return ratio;
            }
            set
            {
                if (!hasMedia) return;
                // convert to time span
                var ticks = (long)(player.NaturalDuration.TimeSpan.Ticks * value);
                player.Position = TimeSpan.FromTicks(ticks);
            }
        }

        public ICommand PlayCommand { get; }

        /// <summary>
        /// toggles current play pause state
        /// </summary>
        public void PlayPause()
        {
            isPausing = !isPausing;
            if (isPausing)
            {
                if(hasMedia) player.Pause();
            }
            else
            {
                if(hasMedia) player.Play();
            }
        }

        public ICommand PreviousEpisodeCommand { get; }

        public ICommand WindBackCommand { get; }

        public ICommand WindForwardCommand { get; }

        /// <summary>
        /// change the current position in this direction
        /// </summary>
        /// <param name="time"></param>
        public void Wind(TimeSpan time)
        {
            player.Position = player.Position.Add(time);
        }

        public ICommand NextEpisodeCommand { get; }

        public ICommand TogglePlaylistCommand { get; }

        public ICommand ToggleFullscreenCommand { get; }

        private void PlayerOnBufferingStarted(object sender, RoutedEventArgs routedEventArgs)
        {
            //int a = 3;
        }

        private void PlayerOnMediaOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            if(!isPausing)
                player.Play();
        }

        private void PlayerOnMediaFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            // cannot find file?
            MessageBox.Show(models.App.TopmostWindow, exceptionRoutedEventArgs.ErrorException.Message, "Playback Error", MessageBoxButton.OK,
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
            }

            player.Stop();
            player.Source = null;
            activePlaylist = models.Playlists.ActivePlaylist;
            if (activePlaylist == null) return;

            activePlaylist.PropertyChanged += ActivePlaylistOnPropertyChanged;
            // where did we left of?
            PlayVideo(activePlaylist.ActiveVideo);
        }

        private void ActivePlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.ActiveVideo):
                    PlayVideo(activePlaylist.ActiveVideo);
                    break;
            }
        }

        public void PlayVideo(VideoModel video)
        {
            // unsubsribe from old event
            if (activeVideo != null) activeVideo.DownloadTask.PropertyChanged -= DownloadTaskOnPropertyChanged;

            activeVideo = video;
            player.Source = null;
            if (activeVideo == null)
            {
                models.Player.VideoName = "";
                EpisodeTitle = "";
                return;
            }

            models.Player.VideoName = video.Name;
            EpisodeTitle = video.Name;

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
                            MessageBox.Show(models.App.TopmostWindow ,"download failed");
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

