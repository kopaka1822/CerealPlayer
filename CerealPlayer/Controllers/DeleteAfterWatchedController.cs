using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Controllers
{
    public class DeleteAfterWatchedController
    {
        private readonly Models.Models models;
        private PlaylistModel activePlaylist = null;
        private VideoModel playingVideo = null;

        public DeleteAfterWatchedController(Models.Models models)
        {
            this.models = models;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }

        /// <summary>
        ///     deletes all videos with a running delete video task
        /// </summary>
        public void Dispose()
        {
            if (!models.Settings.DeleteAfterWatching) return;

            // finish started delete tasks
            foreach (var playlist in models.Playlists.List)
            {
                for (var i = 0; i < playlist.Videos.Count; ++i)
                {
                    var video = playlist.Videos.ElementAt(i);
                    if (video.DeleteTask.Status != TaskModel.TaskStatus.Running) continue;

                    var deleted = playlist.DeleteEpisode(video);
                    Debug.Assert(deleted);

                    // reset index
                    --i;

                    try
                    {
                        if (File.Exists(video.FileLocation))
                        {
                            File.Delete(video.FileLocation);
                        }
                    }
                    catch (Exception)
                    {
                        // ignore 
                    }
                }
            }
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged -= ActivePlaylistOnPropertyChanged;
                        playingVideo = null;
                    }

                    activePlaylist = models.Playlists.ActivePlaylist;
                    if (activePlaylist != null)
                    {
                        activePlaylist.PropertyChanged += ActivePlaylistOnPropertyChanged;
                        playingVideo = activePlaylist.PlayingVideo;
                    }

                    break;
            }
        }

        private void ActivePlaylistOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.PlayingVideo):
                    // schedule deletion of previous video
                    if (models.Settings.DeleteAfterWatching && playingVideo != null)
                    {
                        if (playingVideo.DeleteTask.Status == TaskModel.TaskStatus.Failed
                            || playingVideo.DeleteTask.Status == TaskModel.TaskStatus.ReadyToStart)
                        {
                            // only delete if the new video has a higher index than the previous one
                            if (activePlaylist.PlayingVideo == null
                                || activePlaylist.PlayingVideo.Number > playingVideo.Number)
                            {
                                playingVideo.DeleteTask.Start();
                            }
                        }
                    }

                    playingVideo = activePlaylist.PlayingVideo;
                    // stop deletion from this video
                    if (playingVideo != null)
                    {
                        if (playingVideo.DeleteTask.ReadyOrRunning)
                            playingVideo.DeleteTask.Stop();
                    }

                    break;
            }
        }
    }
}