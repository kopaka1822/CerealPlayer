using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;
using Newtonsoft.Json;

namespace CerealPlayer.Controllers
{
    public class SaveFileController
    {
        private readonly Models.Models models;
        private PlaylistModel activePlaylist = null;

        public SaveFileController(Models.Models models)
        {
            this.models = models;
            this.models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    // save state after playlist changed
                    if (activePlaylist != null)
                    {
                        SavePlaylist(activePlaylist);
                        activePlaylist.PropertyChanged -= ActivePlaylistOnPropertyChanged;
                    }

                    activePlaylist = models.Playlists.ActivePlaylist;
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged += ActivePlaylistOnPropertyChanged;
                    }

                    break;
            }
        }

        private void ActivePlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideo):
                    // save state after video changed
                    SavePlaylist(activePlaylist);
                    break;
            }
        }

        /// <summary>
        ///     will be called after a playlist was added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PlaylistOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems)
                {
                    var playlist = (PlaylistModel) item;
                    SavePlaylist(playlist);
                    // save playlist after a video finished downloading
                    playlist.VideosCollectionChanged += PlaylistOnVideosCollectionChanged;
                    foreach (var playlistVideo in playlist.Videos)
                    {
                        AddVideoDownloadFinishedHandler(playlistVideo);
                    }
                }
            }
        }

        /// <summary>
        ///     will be called after the vide collection of any playlist is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PlaylistOnVideosCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null) return;
            foreach (var item in args.NewItems)
            {
                var video = (VideoModel) item;
                AddVideoDownloadFinishedHandler(video);
            }
        }

        /// <summary>
        ///     adds a handler that will save the playlist after the video was sucesfully downloaded
        /// </summary>
        /// <param name="video"></param>
        private void AddVideoDownloadFinishedHandler(VideoModel video)
        {
            if (video.DownloadTask.Status == TaskModel.TaskStatus.Finished) return;
            video.DownloadTask.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(TaskModel.Status)) return;

                if (video.DownloadTask.Status == TaskModel.TaskStatus.Finished)
                {
                    SavePlaylist(video.Parent);
                }
            };
        }

        public void SavePlaylist(PlaylistModel playlist)
        {
            var data = JsonConvert.SerializeObject(playlist.GetSaveData());
            File.WriteAllText(playlist.SettingsLocation, data);
        }

        /// <summary>
        ///     saves all playlist files
        /// </summary>
        public void Dispose()
        {
            foreach (var playlistModel in models.Playlists.List)
            {
                SavePlaylist(playlistModel);
            }
        }
    }
}