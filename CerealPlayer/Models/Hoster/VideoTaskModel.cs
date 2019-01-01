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
        }

        public VideoModel Video { get; }
    }
}
