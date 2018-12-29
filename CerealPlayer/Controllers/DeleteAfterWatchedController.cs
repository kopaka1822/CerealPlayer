using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        if(playingVideo.DeleteTask.ReadyOrRunning)
                            playingVideo.DeleteTask.Stop();
                    }
                    break;
            }
        }
    }
}
