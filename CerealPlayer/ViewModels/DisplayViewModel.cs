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

namespace CerealPlayer.ViewModels
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public DisplayViewModel(Models.Models models)
        {
            this.models = models;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;
        }

        public Visibility PlaylistVisibility => models.Display.ShowPlaylist ? Visibility.Visible : Visibility.Collapsed;

        public Visibility FullscreenVisibility => models.Display.Fullscreen ? Visibility.Collapsed : Visibility.Visible;

        public WindowStyle WindowStyle => models.Display.Fullscreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;

        private WindowState windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get => windowState;
            set
            {
                if (value == windowState) return;
                windowState = value;
                OnPropertyChanged(nameof(WindowState));

                if (windowState == WindowState.Normal && models.Display.Fullscreen)
                {
                    // someone tried to exit fullscreen
                    models.Display.Fullscreen = false;
                }
            }
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.Fullscreen):
                    OnPropertyChanged(nameof(FullscreenVisibility));
                    OnPropertyChanged(nameof(WindowStyle));
                    WindowState = models.Display.Fullscreen ? WindowState.Maximized : WindowState.Normal;
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
