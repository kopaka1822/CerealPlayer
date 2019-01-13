using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using CerealPlayer.Annotations;
using CerealPlayer.Models;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.ViewModels
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private PlaylistModel activePlaylist = null;
        private WindowState windowState = WindowState.Normal;

        private WindowState windowStateBeforeFullscreen = WindowState.Normal;

        public DisplayViewModel(Models.Models models)
        {
            this.models = models;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }

        public string WindowTitle
        {
            get
            {
                if (activePlaylist == null)
                    return "Cereal Player";
                if (activePlaylist.PlayingVideo == null)
                    return "Cereal Player " + activePlaylist.Name;
                return "Cereal Player " + activePlaylist.PlayingVideo.Name;
            }
        }

        public Visibility PlaylistVisibility => models.Display.ShowPlaylist ? Visibility.Visible : Visibility.Collapsed;

        public Visibility FullscreenVisibility => models.Display.Fullscreen ? Visibility.Collapsed : Visibility.Visible;

        public WindowStyle WindowStyle => models.Display.Fullscreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;

        public WindowState WindowState
        {
            get => windowState;
            set
            {
                if (value == windowState) return;
                windowState = value;
                OnPropertyChanged(nameof(WindowState));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged -= ActivePlaylistOnPropertyChanged;
                    }

                    activePlaylist = models.Playlists.ActivePlaylist;
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged += ActivePlaylistOnPropertyChanged;
                    }

                    OnPropertyChanged(nameof(WindowTitle));
                    break;
            }
        }

        private void ActivePlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideo):
                    OnPropertyChanged(nameof(WindowTitle));
                    break;
            }
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.Fullscreen):
                    OnPropertyChanged(nameof(FullscreenVisibility));
                    OnPropertyChanged(nameof(WindowStyle));
                    if (models.Display.Fullscreen)
                    {
                        windowStateBeforeFullscreen = WindowState;
                        if (WindowState == WindowState.Maximized)
                        {
                            // if window state was already maximized we have to switch to another state first..
                            WindowState = WindowState.Minimized;
                        }

                        WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        WindowState = windowStateBeforeFullscreen;
                    }

                    break;
                case nameof(DisplayModel.ShowPlaylist):
                    OnPropertyChanged(nameof(PlaylistVisibility));
                    break;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}