using System;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Models.Hoster
{
    public class VideoTaskModel : TaskModel
    {
        public VideoTaskModel(VideoModel parent) : base(5, TimeSpan.Zero)
        {
            Video = parent;
            Playlist = parent.Parent;
        }

        public VideoModel Video { get; }
        public PlaylistModel Playlist { get; }
        public HosterPreferences Hoster => Playlist.Settings.HosterPreferences;
    }
}