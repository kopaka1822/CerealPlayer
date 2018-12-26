using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CerealPlayer.Annotations;
using CerealPlayer.Models;
using CerealPlayer.Models.Player;

namespace CerealPlayer.ViewModels
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public DisplayViewModel(Models.Models models)
        {
            this.models = models;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;
            this.models.Player.PropertyChanged += PlayerOnPropertyChanged;
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlayerModel.VideoName):
                    OnPropertyChanged(nameof(WindowTitle));
                    break;
            }
        }

        public string WindowTitle => "Cereal Player" + (models.Player.VideoName.Length > 0? " - " + models.Player.VideoName : "");

        public Visibility PlaylistVisibility => models.Display.ShowPlaylist ? Visibility.Visible : Visibility.Collapsed;

        public Visibility FullscreenVisibility => models.Display.Fullscreen ? Visibility.Collapsed : Visibility.Visible;

        public WindowStyle WindowStyle => models.Display.Fullscreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;

        private WindowState windowStateBeforeFullscreen = WindowState.Normal;
        private WindowState windowState = WindowState.Normal;
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
