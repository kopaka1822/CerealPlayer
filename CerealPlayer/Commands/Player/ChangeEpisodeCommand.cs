using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Player
{
    public class ChangeEpisodeCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly int direction;
        private PlaylistModel prevPlaylist = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="direction">-1 for one episode back, +1 for next episode</param>
        public ChangeEpisodeCommand(Models.Models models, int direction)
        {
            this.models = models;
            this.direction = direction;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    OnCanExecuteChanged();
                    // unsubsribe from old event
                    if (prevPlaylist != null)
                    {
                        prevPlaylist.Videos.CollectionChanged -= VideosOnCollectionChanged;
                    }

                    prevPlaylist = models.Playlists.ActivePlaylist;
                    // subscribe to new event
                    if (prevPlaylist != null)
                    {
                        prevPlaylist.Videos.CollectionChanged += VideosOnCollectionChanged;
                    }
                    break;
            }
        }

        private void VideosOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnCanExecuteChanged();
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

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
