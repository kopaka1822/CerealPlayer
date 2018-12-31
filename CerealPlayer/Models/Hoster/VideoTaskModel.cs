using System;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Models.Task.Hoster
{
    public class VideoTaskModel : TaskModel
    {
        public VideoTaskModel(VideoModel parent) : base(5, TimeSpan.Zero)
        {
            Video = parent;
        }

        public VideoModel Video { get; }
    }
}
