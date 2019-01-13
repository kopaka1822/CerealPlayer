using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Player
{
    public class ChangeEpisodeCommand : ICommand
    {
        private readonly int direction;
        private readonly Models.Models models;
        private PlaylistModel prevPlaylist = null;

        /// <summary>
        /// </summary>
        /// <param name="models"></param>
        /// <param name="direction">-1 for one episode back, +1 for next episode</param>
        public ChangeEpisodeCommand(Models.Models models, int direction)
        {
            this.models = models;
            this.direction = direction;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }


        public bool CanExecute(object parameter)
        {
            var pl = models.Playlists.ActivePlaylist;
            if (pl == null) return false;
            var nextIdx = pl.PlayingVideoIndex + direction;
            return nextIdx >= 0 && nextIdx < pl.Videos.Count;
        }

        public void Execute(object parameter)
        {
            models.Playlists.ActivePlaylist.PlayingVideoIndex += direction;
        }

        public event EventHandler CanExecuteChanged;

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    OnCanExecuteChanged();
                    // unsubsribe from old event
                    if (prevPlaylist != null)
                    {
                        prevPlaylist.VideosCollectionChanged -= VideosOnCollectionChanged;
                        prevPlaylist.PropertyChanged -= PlaylistOnPropertyChanged;
                    }

                    prevPlaylist = models.Playlists.ActivePlaylist;
                    // subscribe to new event
                    if (prevPlaylist != null)
                    {
                        prevPlaylist.VideosCollectionChanged += VideosOnCollectionChanged;
                        prevPlaylist.PropertyChanged += PlaylistOnPropertyChanged;
                    }

                    break;
            }
        }

        private void PlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideoIndex):
                    OnCanExecuteChanged();
                    break;
            }
        }

        private void VideosOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnCanExecuteChanged();
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}