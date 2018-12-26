using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Models.Task
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
