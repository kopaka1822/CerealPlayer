using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public Visibility PlayVisibility => isPausing ? Visibility.Collapsed : Visibility.Visible;

        public Visibility PauseVisibility => isPausing ? Visibility.Visible : Visibility.Collapsed;

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

        public Visibility BarVisibility { get; } = Visibility.Visible;

        private void PlayerOnBufferingStarted(object sender, RoutedEventArgs routedEventArgs)
        {
            int a = 3;
        }

        private void PlayerOnMediaOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            if(!isPausing)
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
            if (activeVideo == null)
            {
                models.Player.VideoName = "";
                return;
            }

            models.Player.VideoName = video.Name;

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

