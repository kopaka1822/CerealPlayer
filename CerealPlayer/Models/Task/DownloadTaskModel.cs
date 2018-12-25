using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Models.Task
{
    public class DownloadTaskModel : TaskModel
    {
        public DownloadTaskModel(VideoModel parent)
        {
            Video = parent;
        }

        public VideoModel Video { get; }
    }
}
