using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Views;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public PlaylistViewModel(Models.Models models)
        {
            this.models = models;
            this.models.PropertyChanged += ModelsOnPropertyChanged;
            if(models.Playlist != null)
                HandleNewPlaylist();
        }

        private void ModelsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(Models.Models.Playlist):
                    // new playlist was added!
                    HandleNewPlaylist();
                    break;
            }
        }

        private void HandleNewPlaylist()
        {
            Reset();
            if (models.Playlist == null) return;

            // add existing videos
            foreach (var playlistVideo in models.Playlist.Videos)
            {
                var view = new PlaylistItemView {DataContext = new PlaylistItemViewModel(models, playlistVideo)};
                Videos.Add(view);
            }

            // add future videos
            models.Playlist.Videos.CollectionChanged += VideosOnCollectionChanged;
        }

        private void VideosOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            Debug.Assert(args.Action == NotifyCollectionChangedAction.Add);
            Debug.Assert(args.NewStartingIndex == Videos.Count);

            // add to the end of the list
            Videos.Add(new PlaylistItemView
            {
                DataContext = new PlaylistItemViewModel(models, models.Playlist.Videos.Last())
            });
        }

        private void Reset()
        {
            Videos.Clear();
            SelectedVideo = null;
        }

        public ObservableCollection<PlaylistItemView> Videos { get; } = new ObservableCollection<PlaylistItemView>();

        private PlaylistItemView selectedVideo = null;

        public PlaylistItemView SelectedVideo
        {
            get => selectedVideo;
            set
            {
                if (ReferenceEquals(value, selectedVideo)) return;
                selectedVideo = value;
                OnPropertyChanged(nameof(SelectedVideo));
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
