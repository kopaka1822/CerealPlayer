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
using CerealPlayer.Models.Player;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.ViewModels.Player
{
    public class PlayerViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private PlaylistModel activePlaylist = null;

        public PlayerViewModel(Models.Models models)
        {
            this.models = models;
            this.models.Player.PropertyChanged += PlayerOnPropertyChanged;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;

            PlayCommand = new PlayPauseCommand(models);

            PreviousEpisodeCommand = new ChangeEpisodeCommand(models, -1);
            NextEpisodeCommand = new ChangeEpisodeCommand(models, 1);

            WindBackCommand = new WindPlayerCommand(models, TimeSpan.FromSeconds(-10));
            WindForwardCommand = new WindPlayerCommand(models, TimeSpan.FromSeconds(30));

            ToggleFullscreenCommand = new ToggleFullscreenCommand(models);
            TogglePlaylistCommand = new TogglePlaylistCommand(models);

            // allow user to modify slider
            var slider = models.App.Window.PlayerBar.Slider;
            slider.PreviewMouseDown += (sender, args) => userHoldsTime = true;
            slider.PreviewMouseUp += (sender, args) => userHoldsTime = false;
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged -= ActivePlaylistOnPropertyChanged;
                        activePlaylist.PlayingVideoPositionChanged -= ActivePlaylistOnPlayingVideoPositionChanged;
                    }
                    activePlaylist = models.Playlists.ActivePlaylist;
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged += ActivePlaylistOnPropertyChanged;
                        activePlaylist.PlayingVideoPositionChanged += ActivePlaylistOnPlayingVideoPositionChanged;
                    }
                    OnPropertyChanged(nameof(EpisodeTitle));
                    RaiseTimeChangeEvents();
                    break;
            }
        }

        private void ActivePlaylistOnPlayingVideoPositionChanged(object sender, EventArgs eventArgs)
        {
            RaiseTimeChangeEvents();
        }

        private void RaiseTimeChangeEvents()
        {
            OnPropertyChanged(nameof(TimeElapsed));
            OnPropertyChanged(nameof(TimeRemaining));
            if(!userHoldsTime)
                OnPropertyChanged(nameof(TimeProgress));
        }

        private void ActivePlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideo):
                    OnPropertyChanged(nameof(EpisodeTitle));
                    RaiseTimeChangeEvents();
                    break;
            }
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlayerModel.IsPausing):
                    OnPropertyChanged(nameof(PlayVisibility));
                    OnPropertyChanged(nameof(PauseVisibility));
                    break;
                case nameof(PlayerModel.Volume):
                    OnPropertyChanged(nameof(Volume));
                    break;
                case nameof(PlayerModel.IsPlayerBarVisible):
                    OnPropertyChanged(nameof(BarVisibility));
                    break;
            }
        }

        public Visibility BarVisibility => models.Player.IsPlayerBarVisible ? Visibility.Visible : Visibility.Collapsed;

        public Visibility PauseVisibility => models.Player.IsPausing ? Visibility.Collapsed : Visibility.Visible;

        public Visibility PlayVisibility => models.Player.IsPausing ? Visibility.Visible : Visibility.Collapsed;

        public string EpisodeTitle
        {
            get
            {
                if (activePlaylist == null) return "";
                if (activePlaylist.PlayingVideo == null) return "waiting for next episode";
                return activePlaylist.PlayingVideo.Name;
            }
        }

        public double Volume
        {
            get => models.Player.Volume;
            set => models.Player.Volume = value;
        }

        private readonly string noMediaTime = "--:--:--";

        private bool HasMedia => activePlaylist?.PlayingVideo != null;

        public string TimeElapsed => HasMedia ? activePlaylist.PlayingVideoPosition.ToString(@"hh\:mm\:ss") : noMediaTime;

        public string TimeRemaining
        {
            get
            {
                if (!HasMedia) return noMediaTime;
                // duration set?
                if (activePlaylist.PlayingVideoDuration == TimeSpan.Zero) return noMediaTime;

                var pos = activePlaylist.PlayingVideoPosition;
                var dur = activePlaylist.PlayingVideoDuration;
                var res = dur.Subtract(pos);
                return res.ToString(@"hh\:mm\:ss");
            }
        }

        // this variable will be used if the user is holding down the trackbar
        private double userTimeProgress = 0;
        private bool userHoldsTime = false;

        public double TimeProgress
        {
            get
            {
                if (!HasMedia) return 0;
                if (activePlaylist.PlayingVideoDuration == TimeSpan.Zero) return 0;
                if (userHoldsTime) return userTimeProgress;

                return (double) activePlaylist.PlayingVideoPosition.Ticks / activePlaylist.PlayingVideoDuration.Ticks;
            }
            set
            {
                if(!HasMedia) return;
                if(activePlaylist.PlayingVideoDuration == TimeSpan.Zero) return;
                var ticks = (long) (activePlaylist.PlayingVideoDuration.Ticks * value);
                var newPos = TimeSpan.FromTicks(ticks);

                userTimeProgress = (double)newPos.Ticks / activePlaylist.PlayingVideoDuration.Ticks;

                activePlaylist.PlayingVideoPosition = newPos;
            }
        }

        public ICommand PlayCommand { get; }

        public ICommand PreviousEpisodeCommand { get; }

        public ICommand WindBackCommand { get; }

        public ICommand WindForwardCommand { get; }

        public ICommand NextEpisodeCommand { get; }

        public ICommand TogglePlaylistCommand { get; }

        public ICommand ToggleFullscreenCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

